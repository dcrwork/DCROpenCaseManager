using Moq;
using OpenCaseManager.Commons;
using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace OpenCaseManagerTests
{
    public class RecordsControllerTests
    {
        [Fact]
        public void AddJournalHistory_correct_input_verify_DataManager_is_called()
        {
            var manager =  new Mock<IManager>();
            var dataManager = new Mock<IDataModelManager>();
            manager.Setup(s => s.InsertData(dataManager.Object.DataModel));

            Common.AddJournalHistory("1", "1", null, "Event", "Slicing pizzas", DateTime.Now, "1", manager.Object, dataManager.Object);

            manager.Verify(m => m.InsertData(dataManager.Object.DataModel));
        }

        [Fact]
        public void AddJournalHistory_given_null_EventId_does_not_add_it_as_parameter()
        {
            var manager = new Mock<IManager>();
            var dataManager = new Mock<IDataModelManager>();
            manager.Setup(s => s.InsertData(dataManager.Object.DataModel));
            dataManager.Setup(s => s.AddParameter(DBEntityNames.JournalHistory.EventId.ToString(), Enums.ParameterType._int, null));

            Common.AddJournalHistory("1", null, "42", "Event", "Slicing pizzas", DateTime.Now, "1", manager.Object, dataManager.Object);

            dataManager.Verify(dm => dm.AddParameter(DBEntityNames.JournalHistory.EventId.ToString(), Enums.ParameterType._int, null), Times.Never());
        }

        [Fact]
        public void AddJournalHistory_given_no_title_throws_an_exception()
        {
            var manager = new Mock<IManager>();
            var dataManager = new Mock<IDataModelManager>();
            string instanceId = null;
            manager.Setup(s => s.InsertData(dataManager.Object.DataModel));

            Assert.Throws<NullReferenceException>(() => Common.AddJournalHistory(instanceId, null, "42", "Event", "Wuhu", DateTime.Now, "1", manager.Object, dataManager.Object));
        }
    }
}