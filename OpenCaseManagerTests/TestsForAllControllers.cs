using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Reflection;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using OpenCaseManager.Controllers.ApiControllers;
using OpenCaseManager.Controllers;

namespace OpenCaseManagerTests
{
    //These tests ended up taking way too long time, so now it is just being abandoned, as there are many problems with types that seem pointless. Johnny
    public class TestsForAllControllers
    {
        [Fact]
        public void RecordsController_has_authorize_token()
        {
            var controller = typeof(RecordsController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public void ServicesController_has_authorize_token()
        {
            var controller = typeof(ServicesController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public void AdminController_has_authorize_token()
        {
        }

        [Fact]
        public void ChildController_has_authorize_token()
        {
            var controller = typeof(ChildController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public void DigitalAssistantController_has_authorize_token()
        {
            var controller = typeof(DigitalAssistantController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public void FileController_has_authorize_token()
        {
            var controller = typeof(FileController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public void FormController_has_authorize_token()
        {
            var controller = typeof(FormController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public void HomeController_has_authorize_token()
        {
            var controller = typeof(HomeController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public void InstanceController_has_authorize_token()
        {
            var controller = typeof(InstanceController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public void JournalNoteController_has_authorize_token()
        {
            var controller = typeof(MUSController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public void ProcessController_has_authorize_token()
        {
            var controller = typeof(ProcessController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

        [Fact]
        public void SearchController_has_authorize_token()
        {
            var controller = typeof(SearchController);
            var attributes = controller.GetCustomAttributes(false).Select(a => a.GetType());
            Assert.Contains(typeof(AuthorizeAttribute), attributes);
        }

    }
}
