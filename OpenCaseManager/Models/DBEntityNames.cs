using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Models
{
    public static class DBEntityNames
    {
        public enum Tables
        {
            AllInstances,
            Child,
            Event,
            EventHistory,
            DocumentTimes,
            Form,
            FormItem,
            Instance,
            InstanceAutomaticEvents,
            InstanceEventHistory,
            InstanceEvents,
            InstancePhases,
            InstanceRole,
            JournalHistory,
            Log,
            MUS,
            MyInstances,
            PhaseInstances,
            Process,
            ProcessPhase,
            ResponsibleInstancesCount,
            User,
            UserDetail,
            InstanceExtension,
            AcadreLog,
            Document
        }

        public enum StoredProcedures
        {
            AdvanceTime,
            AddEventTypeData,
            AddInstanceRoles,
            AddProcessPhases,
            SetCurrentPhase,
            SyncEvents,
            UpdateEventLogInstance,
            SetFormItemSequence,
            DeleteFormItem,
            CopyFormFromTemplate,
            AddInstanceDescription,
            LockDocuments
        }

        public enum Functions
        {
            fn_Split,
            InstanceTasksAllEnabled,
            InstanceTasks
        }

        public enum AllInstances
        {
            Instance,
            Sequence,
            EventId,
            Status,
            Description,
            Details
        }

        public enum Event
        {
            Id,
            InstanceId,
            EventId,
            Title,
            Responsible,
            Due,
            PhaseId,
            IsEnabled,
            IsPending,
            IsIncluded,
            IsExecuted,
            EventType,
            isOpen,
            Description,
            EventTypeData
        }

        public enum EventHistory
        {
            InstanceId,
            Sequence,
            EventId,
            Status,
            Description,
            Details,
            ExecutionDate
        }

        public enum Form
        {
            Id,
            FormTemplateId,
            Title,
            IsTemplate,
            UserId
        }

        public enum FormItem
        {
            Id,
            FormId,
            IsGroup,
            ItemId,
            SequenceNumber,
            ItemText
        }

        public enum DocumentTimes
        {
            Id,
            Title,
            Type,
            Link,
            Responsible,
            InstanceId,
            UploadDate,
            IsLocked,
            IsDraft,
            EventDate
        }

        public enum Instance
        {
            Id,
            GraphId,
            SimulationId,
            Title,
            CaseNoForeign,
            CaseLink,
            Responsible,
            IsAccepting,
            CurrentPhaseNo,
            NextDelay,
            NextDeadline,
            IsOpen,
            DCRXML,
            CaseStatus,
            InternalCaseID,
            Description
        }

        public enum InstanceAutomaticEvents
        {
            EventId,
            EventTitle,
            EventOpen,
            IsEnabled,
            IsPending,
            IsIncluded,
            IsExecuted,
            EventType,
            InstanceId,
            Responsible,
            EventTypeData
        }

        public enum InstanceEventHistory
        {
            Id,
            CaseId,
            CaseStatus,
            EventId,
            Sequence,
            Description,
            Status,
            CaseTitle,
            GraphId,
            Details
        }

        public enum InstanceEvents
        {
            EventId,
            EventTitle,
            Responsible,
            Name,
            Due,
            EventIsOpen,
            IsEnabled,
            IsPending,
            IsIncluded,
            IsExecuted,
            EventType,
            InstanceId,
            SimulationId,
            GraphId,
            Description,
            Case,
            CaseLink,
            CaseTitle,
            InstanceIsOpen,
            IsUIEvent,
            UIEventValue
        }

        public enum InstancePhases
        {
            ProcessId,
            GraphId,
            InstanceId,
            Title,
            SequenceNumber,
            CurrentPhase,
            PhaseId
        }

        public enum InstanceRole
        {
            Id,
            InstanceId,
            Role,
            UserId
        }

        public enum Log
        {
            Id,
            Logged,
            Level,
            UserName,
            ServerName,
            Port,
            Url,
            Https,
            Message,
            Exception
        }

        public enum MUS
        {
            Id,
            Username,
            FullName,
            InstanceTitle,
            CaseNoForeign,
            CaseLink,
            CurrentPhaseNo,
            SimulationId,
            InstanceId,
            ManagerId,
            Department
        }

        public enum MyInstances
        {
            Id,
            GraphId,
            SimulationId,
            Title,
            CaseNoForeign,
            CaseLink,
            Responsible,
            IsAccepting,
            CurrentPhaseNo,
            NextDelay,
            NextDeadline,
            IsOpen
        }

        public enum PhaseInstances
        {
            Id,
            InstanceCount,
            Title,
            ProcessId,
            Responsible
        }

        public enum Process
        {
            Id,
            GraphId,
            Title,
            ForeignIntegration,
            DCRXML,
            Status,
            Modified,
            Created,
            OnFrontPage
        }

        public enum ProcessPhase
        {
            Id,
            ProcessId,
            SequenceNumber,
            Title
        }

        public enum ResponsibleInstancesCount
        {
            Id,
            Name,
            Count,
            GraphId
        }

        public enum User
        {
            Id,
            SamAccountName,
            Name,
            Title,
            Department,
            ManagerId
        }

        public enum UserDetail
        {
            Id,
            SamAccountName,
            Name,
            Title,
            Department,
            ManagerId,
            IsManager
        }

        public enum AddEventTypeData
        {
            InstanceId
        }

        public enum AddInstanceRoles
        {
            InstanceId,
            UserRoles
        }

        public enum AddProcessPhases
        {
            ProcessId,
            PhaseXml
        }

        public enum CopyFormFromTemplate
        {
            FormId,
            TemplateId
        }

        public enum DeleteFormItem
        {
            FormItemId
        }

        public enum SetCurrentPhase
        {
            instanceId
        }

        public enum SetFormItemSequence
        {
            Source,
            Target,
            Position
        }

        public enum SyncEvents
        {
            InstanceId,
            EventXML,
            LoginUser
        }

        public enum UpdateEventLogInstance
        {
            instanceId,
            xml
        }

        public enum InstanceExtension
        {
            Id,
            InstanceId,
            Employee,
            Year,
            ChildId
        }

        public enum Child
        {
            Id,
            Name,
            Responsible
        }

        public enum AcadreLog
        {
            Id,
            Method,
            Parameters,
            IsSuccess,
            Result,
            ErrorStatement,
            ErrorStackTrace,
            InstanceId,
            Created
        }

        public enum Document
        {
            Id,
            Title,
            Type,
            Link,
            Responsible,
            InstanceId,
            IsActive,
            UploadDate,
            IsLocked,
            IsDraft
        }

        public enum AddInstanceDescription
        {
            GraphId,
            InstanceId
        }

        public enum JournalHistory
        {
            Id,
            InstanceId,
            EventId,
            DocumentId,
            Type,
            Title,
            CreationDate,
            EventDate,
            IsLocked
        }
    }
}