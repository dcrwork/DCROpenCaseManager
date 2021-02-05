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
            Adjunkt,
            AdjunktUser,
            AdjunktView,
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
            MineAktiviteter,
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
            Document,
            StamdataChild,
            StamdataDummyData,
            StamdataDummyDataExtension,
            ProcessHistory,
            ContextInstances,
            TestCase,
            TestCaseInstance,
            InstanceAIRoboticEvents
        }

        public enum AnonymousTables
        {
            InstanceEvents
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
            LockDocuments,
            GetMenuItems,
            OCMSpawnChildProcessCopyContext,
            CanExecuteGlobalEvent,
            GetGlobalEvents,
            ChangeResponsibleOfChild,
            GetDCRXMLLog,
            ReleaseProcessInstance,
            GetEventResponsibleRoles,
            DeleteTestCase,
            GetTestCases,
            InstanceTasksAI
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
        
        public enum Adjunkt
        {
            Id,
            Responsible,
            Name,
            ObsBoxText
        }

        public enum AdjunktUser
        {
            Id, 
            AdjunktId,
            UserId
        }

        public enum AdjunktView
        {
            Id, 
            Responsible,
            Name,
            ResponsibleName
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
            EventTypeData,
            Note,
            NoteIsHtml,
            NotApplicable,
            Roles
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
            EventTypeData,
            Modified,
            GraphId,
            SimulationId,
            TrueEventId,
            Description
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

        public enum InstanceTask
        {
            EventId,
            TrueEventId,
            Responsible,
            InstanceId,
            EventTitle,
            SimulationId,
            GraphId,
            IsPending,
            IsExecuted,
            CanExecute,
            ResponsibleName,
            Description,
            IsUIEvent,
            UIEventValue,
            EventType,
            Type,
            Case,
            CaseLink,
            CaseTitle,
            IsOverDue,
            DaysPassedDue,
            NotApplicable,
            ParentId,
            Roles,
            Due,
            Modified,
            ActualIsPending,
            ActualIsEnabled,
            ActualIsExecuted,
            IsEnabled,
            RoleToTest
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

        public enum MineAktiviteter
        {
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
            Type,
            EventType,
            isOpen,
            Description,
            EventTypeData,
            Note,
            NoteIsHtml,
            NotApplicable,
            Roles,
            ParentId,
            ChildId, 
            SamAccountName,
            Name,
            UserTitle,
            ManagerId,
            Acadreorgid, 
            DepartmentId,
            IsManager,
            Familieafdelingen,
            Department,
            InstanceTitle,
            Modified,
            GraphId,
            SimulationId,
            TrueEventId,
            EventTitle
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
            OnFrontPage,
            Guid,
            CreateInstance,
            EventId,
            InstanceGuid,
            MajorVersionId,
            MajorVersionTitle,
            MajorVerisonDate,
            ReleaseDate,
            Owner
        }

        public enum ProcessHistory
        {
            Id,
            GraphId,
            Title,
            ForeignIntegration,
            DCRXML,
            Status,
            Modified,
            Created,
            OnFrontPage,
            Guid,
            CreateInstance,
            EventId,
            InstanceGuid,
            MajorVersionId,
            MajorVersionTitle,
            MajorVerisonDate,
            ReleaseDate,
            Owner,
            InstanceId,
            State
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
            ManagerId,
            Acadreorgid,
            DepartmentId,
            IsManager
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

        public enum ReleaseProcessInstance
        {
            InstanceId,
            ProcessPhaseXML
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

        public enum DeleteTestCase
        {
            Id
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
            Responsible,
            ObsBoxText
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
            IsDraft,
            ChildId
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
            ChildId,
            Type,
            Title,
            CreationDate,
            EventDate,
            IsLocked,
            ResponsibleInitials,
            ResponsibleName
        }

        public enum StamdataChild
        {
            Id,
            ChildId,
            Sagsnummer,
            Addresse,
            Forældremyndighed,
            Skole,
            Alder,
            Navn
        }

        public enum StamdataDummyData
        {
            Id,
            CPR,
            Address,
            City,
            Postcode
        }

        public enum StamdataDummyDataExtension
        {
            Id,
            ChildId,
            StamdataId,
            Relation
        }

        public enum OCMSpawnChildProcessCopyContext
        {
            InstanceId,
            ParentInstanceId
        }

        public enum CanExecuteGlobalEvent
        {
            ChildId,
            EventId
        }

        public enum GetGlobalEvents
        {
            ChildId,
            EventId
        }

        public enum ChangeResponsibleOfChild
        {
            ChildId,
            InstanceId,
            EventId,
            FromInitials,
            ToInitials
        }

        public enum GetDCRXMLLog
        {
            GraphId,
            From,
            To,
            IsAccepting,
            TestId
        }

        public enum ContextInstances
        {
            InstanceId,
            ChildId,
            GraphId
        }

        public enum GetEventResponsibleRoles
        {
            InstanceId,
            EventId,
            ResponsibleId,
            EventsXML
        }

        public enum TestCase
        {
            Id,
            Created,
            CreatedBy,
            Modified,
            ModifiedBy,
            Title,
            Description,
            Guid,
            ValidFrom,
            ValidTo,
            Status,
            DCRGraphId,
            Delay,
            RoleToTest,
            Page
        }

        public enum TestCaseInstance
        {
            Id,
            Created,
            TestCaseId,
            Name,
            Email,
            InstanceId
        }
    }
}