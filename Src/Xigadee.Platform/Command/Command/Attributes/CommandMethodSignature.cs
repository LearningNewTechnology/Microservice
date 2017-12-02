﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Xigadee
{
    /// <summary>
    /// This class is used to marshal the method signature
    /// </summary>
    [DebuggerDisplay("{Method.Name}")]
    public class CommandMethodSignature: CommandSignatureBase
    {
       
        /// <summary>
        /// Specifies whether this is a generic signature.
        /// </summary>
        public bool IsStandardCall { get; private set; }
        /// <summary>
        /// This specifies whether the method call is async.
        /// </summary>
        public bool IsAsync { get; private set; }
        /// <summary>
        /// this property specifies whether the return value is the response parameter.
        /// </summary>
        public bool IsReturnValue { get; private set; }

        ///// <summary>
        ///// These are the assigned command attributes.
        ///// </summary>
        //public List<A> CommandAttributes { get; protected set; }
        /// <summary>
        /// This is the list of the parameters for the method.
        /// </summary>
        public List<ParameterInfo> Parameters { get; protected set; }

        /// <summary>
        /// This method validates the method.
        /// </summary>
        /// <param name="throwException">Set this to true throw an exception if the signature does not match,</param>
        /// <returns>Returns true if the signature is validated.</returns>
        protected override bool Validate(bool throwException = false)
        {
            try
            {
                //OK, check whether the return parameter is a Task or Task<> async construct
                IsAsync = typeof(Task).IsAssignableFrom(Method.ReturnParameter.ParameterType);

                Parameters = Method.GetParameters().ToList();
                var paramInfo = Method.GetParameters().ToList();

                //OK, see if the standard parameters exist and aren't decorated as In or Out.
                StandardIn = paramInfo
                    .Where((p) => !ParamAttributes<PayloadInAttribute>(p))
                    .Where((p) => !ParamAttributes<PayloadOutAttribute>(p))
                    .FirstOrDefault((p) => p.ParameterType == typeof(TransmissionPayload));
                bool isStandardIn = (StandardIn != null) && paramInfo.Remove(StandardIn);
                if (StandardIn != null)
                    StandardInPos = Parameters.IndexOf(StandardIn);

                StandardOut = paramInfo
                    .Where((p) => !ParamAttributes<PayloadInAttribute>(p))
                    .Where((p) => !ParamAttributes<PayloadOutAttribute>(p))
                    .FirstOrDefault((p) => p.ParameterType == typeof(List<TransmissionPayload>));
                bool isStandardOut = (StandardOut != null) && paramInfo.Remove(StandardOut);
                if (StandardOut != null)
                    StandardOutPos = Parameters.IndexOf(StandardOut);

                IsStandardCall = (isStandardIn || isStandardOut) && paramInfo.Count == 0;

                if (IsStandardCall)
                    return true;

                //Get the In parameter
                ParamIn = Parameters.Where((p) => ParamAttributes<PayloadInAttribute>(p)).FirstOrDefault();
                if (ParamIn != null && paramInfo.Remove(ParamIn))
                {
                    TypeIn = ParamIn?.ParameterType;
                    ParamInPos = Parameters.IndexOf(ParamIn);
                }

                //Now get the out parameter
                ParamOut = Parameters.Where((p) => ParamAttributes<PayloadOutAttribute>(p)).FirstOrDefault();

                if (ParamOut == null && ParamAttributes<PayloadOutAttribute>(Method.ReturnParameter))
                {
                    ParamOut = Method.ReturnParameter;
                    IsReturnValue = true;
                }
                else if (ParamOut != null && paramInfo.Remove(ParamOut))
                {
                    ParamOutPos = Parameters.IndexOf(ParamOut);
                }

                if (ParamOut != null && !IsReturnValue && !ParamOut.IsOut)
                    if (throwException)
                        throw new CommandContractSignatureException($"Parameter {ParamOut.Name} is not marked as an out parameter.");
                    else
                        return false;

                if (IsAsync && IsReturnValue && ParamOut.ParameterType.IsGenericType)
                {
                    if (ParamOut.ParameterType.GenericTypeArguments.Length != 1)
                        if (throwException)
                            throw new CommandContractSignatureException($"Generic Task response parameter can only have one parameter.");
                        else
                            return false;

                    TypeOut = ParamOut.ParameterType.GenericTypeArguments[0];
                }
                else if (!IsAsync)
                {
                    TypeOut = ParamOut.ParameterType;
                }

                //Finally check that we have used all the parameters.
                if (paramInfo.Count != 0 && throwException)
                    throw new CommandContractSignatureException($"There are too many parameters in the method ({paramInfo[0].Name}).");

                return paramInfo.Count == 0;

            }
            catch (Exception ex)
            {
                throw new CommandContractSignatureException("PayloadIn or PayloadOut have not been set correctly.", ex);
            }
        }

        private bool ParamAttributes<T>(ParameterInfo info)
            where T : Attribute
        {
            try
            {
                var attr = Attribute.GetCustomAttribute(info, typeof(T), false);

                return attr != null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// This is the standard TransmissionPayload parameter in information.
        /// </summary>
        public ParameterInfo StandardIn { get; private set; }
        /// <summary>
        /// This is the standard in parameter position.
        /// </summary>
        public int? StandardInPos { get; private set; }

        /// <summary>
        /// This is the standard out List/<TransmissionPayload/> parameter information.
        /// </summary>
        public ParameterInfo StandardOut { get; private set; }
        /// <summary>
        /// This is the standard out parameter position.
        /// </summary>
        public int? StandardOutPos { get; private set; }

        /// <summary>
        /// This is the in-parameter information.
        /// </summary>
        public ParameterInfo ParamIn { get; private set; }
        /// <summary>
        /// This is the in-parameter position
        /// </summary>
        public int? ParamInPos { get; private set; }

        /// <summary>
        /// This is the in-parameter type.
        /// </summary>
        public Type TypeIn { get; set; }

        /// <summary>
        /// This is the out-parameter information.
        /// </summary>
        public ParameterInfo ParamOut { get; private set; }
        /// <summary>
        /// This is the out-parameter position
        /// </summary>
        public int? ParamOutPos { get; private set; }
        /// <summary>
        /// This is the out parameter type.
        /// </summary>
        public Type TypeOut { get; set; }

        /// <summary>
        /// This is the command action that is executed.
        /// </summary>
        public Func<TransmissionPayload, List<TransmissionPayload>, IPayloadSerializationContainer, Task> Action
        {
            get
            {
                return async (pIn, pOut, ser) =>
                {
                    try
                    {
                        var collection = new object[Parameters.Count];

                        if (ParamInPos.HasValue)
                            collection[ParamInPos.Value] = ser.PayloadDeserialize(pIn.Message);

                        if (StandardInPos.HasValue)
                            collection[StandardInPos.Value] = pIn;

                        if (StandardOutPos.HasValue)
                            collection[StandardOutPos.Value] = pOut;

                        object output = null;

                        if (IsAsync)
                        {
                            if (TypeOut == null)
                                await (Task)Method.Invoke(Command, collection);
                            else
                                output = await (dynamic)Method.Invoke(Command, collection);
                        }
                        else
                        {
                            if (!IsReturnValue)
                            {
                                Method.Invoke(Command, collection);
                                if (ParamOutPos.HasValue)
                                    output = collection[ParamOutPos.Value];
                            }
                            else
                                output = (dynamic)Method.Invoke(Command, collection);
                        }

                        if (TypeOut != null)
                        {
                            var response = pIn.ToResponse();
                            response.Message.Blob = ser.PayloadSerialize(output);
                            response.Message.Status = "200";

                            pOut.Add(response);
                        }
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }

                };
            }
        }
    }
}