﻿//#region Copyright
//// Copyright Hitachi Consulting
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
////    http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.
//#endregion

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Xigadee;

//namespace Test.Xigadee
//{
//    [Contract("Channel2", "Start", "Something")]
//    public interface IStartSomething: IMessageContract {}

//    [Contract("Channel2", "Follow", "Up")]
//    public interface IFollowUp: IMessageContract {}

//    [TestClass]
//    public class Microservice_Validate_CommandRequest: TestPopulator<TestMicroservice, TestConfig>
//    {
//        protected EventTestCommand<IStartSomething> mCommand1;
//        protected EventTestCommand<IFollowUp> mCommand2;

//        protected override void RegisterCommands()
//        {
//            base.RegisterCommands();

//            mCommand1 = (EventTestCommand<IStartSomething>)Service.RegisterCommand(new EventTestCommand<IStartSomething>());
//            mCommand2 = (EventTestCommand<IFollowUp>)Service.RegisterCommand(new EventTestCommand<IFollowUp>());
//        }


//        [TestMethod]
//        public void VerifyCommandCount()
//        {
//            Assert.AreEqual(Service.Commands.Count(),2);
//        }

//        //[TestMethod]
//        //public void IDoSomething1CommandCheck()
//        //{
//        //    bool isSuccess = false;

//        //    try
//        //    {
//        //        ManualResetEvent reset = new ManualResetEvent(false);

//        //        var del = new EventHandler<Tuple<TransmissionPayload, List<TransmissionPayload>>>((sender, e) =>
//        //        {
//        //            isSuccess = true;
//        //            reset.Set();
//        //        });

//        //        mCommand1.OnExecute += del;

//        //        Service.Process<IDoSomething1>(options: ProcessOptions.RouteInternal);
//        //        reset.WaitOne(5000);

//        //        mCommand1.OnExecute -= del;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Assert.Fail(ex.Message);
//        //    }

//        //    Assert.IsTrue(isSuccess);
//        //}

//    }
//}
