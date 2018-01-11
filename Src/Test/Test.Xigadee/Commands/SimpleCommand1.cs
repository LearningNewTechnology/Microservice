﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xigadee;

namespace Test.Xigadee
{
    [TestClass]
    public class SimpleCommand1UnitTest: CommandUnitTestBase<SimpleCommand1>
    {
        [TestMethod]
        public void TestStandard1()
        {
            DefaultTest();
        }

        [TestMethod]
        public void PipelineCommand1()
        {
            IPipeline pipeline = null;
            try
            {
                pipeline = Pipeline();

                pipeline.Start();

                int start = Environment.TickCount;

                var result1 = mCommandInit.Process<Blah, string>("internalIn", "simples1", "async",
                    new Blah() { Message = "hello" }, new RequestSettings() { WaitTime = TimeSpan.FromHours(1) }).Result;

                var result2 = mCommandInit.Process<Blah, string>("internalIn", "simples1", "sync",
                    new Blah() { Message = "hello" }, new RequestSettings() { WaitTime = TimeSpan.FromHours(1) }).Result;

                var end = ConversionHelper.DeltaAsTimeSpan(start);

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            try
            {
                pipeline?.Stop();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

    public class SimpleCommand1: CommandBase
    {
        public SimpleCommand1() : base(null){}

        [CommandContract(messageType: "simples1", actionType: "async")]
        private async Task ActionAsync(TransmissionPayload incoming, List<TransmissionPayload> outgoing)
        {
            var blahIn = PayloadSerializer.PayloadDeserialize<Blah>(incoming);
            var rs = incoming.ToResponse();
            rs.Message.Blob.SetObject("Freaky");
            rs.Message.Status = "204";
            rs.Message.StatusDescription = "Hello";
            outgoing.Add(rs);

            //return Task.FromResult(0);
        }

        [CommandContract(messageType: "simples1", actionType: "sync")]
        private void ActionSync(TransmissionPayload incoming, List<TransmissionPayload> outgoing)
        {
            var blahIn = PayloadSerializer.PayloadDeserialize<Blah>(incoming);
            var rs = incoming.ToResponse();
            rs.Message.Blob = PayloadSerializer.PayloadSerialize(blahIn.Message);
            rs.Message.Status = "204";
            rs.Message.StatusDescription = "Hello";
            outgoing.Add(rs);
        }

        [MasterJobCommandContract(messageType: "simples2", actionType: "async")]
        private async Task ActionAsyncMaster(TransmissionPayload incoming, List<TransmissionPayload> outgoing)
        {
            var blahIn = PayloadSerializer.PayloadDeserialize<Blah>(incoming);
            var rs = incoming.ToResponse();
            //rs.Message.Blob = PayloadSerializer.PayloadSerialize(new Blah { ContentId = blahIn.ContentId, Message = "Howdy" });
            rs.Message.Blob.SetObject("Freaky");
            rs.Message.Status = "204";
            rs.Message.StatusDescription = "Hello";
            outgoing.Add(rs);

            //return Task.FromResult(0);
        }

        [MasterJobCommandContract(messageType: "simples2", actionType: "sync")]
        private void ActionSyncMaster(TransmissionPayload incoming, List<TransmissionPayload> outgoing)
        {
            var blahIn = PayloadSerializer.PayloadDeserialize<Blah>(incoming);
            var rs = incoming.ToResponse();
            rs.Message.Blob = PayloadSerializer.PayloadSerialize(blahIn.Message);
            rs.Message.Status = "204";
            rs.Message.StatusDescription = "Hello";
            outgoing.Add(rs);
        }


    }
}
