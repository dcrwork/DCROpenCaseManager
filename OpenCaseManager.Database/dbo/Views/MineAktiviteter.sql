CREATE VIEW MineAktiviteter AS 
  select e.InstanceId, e.EventId, e.Title, e.Responsible, e.Due, e.PhaseId, e.IsEnabled, e.IsPending, e.IsIncluded, e.IsExecuted, e.EventType, e.isOpen, e.Description, e.EventTypeData, e.Type, e.Token, e.Note, e.NotApplicable, e.NoteIsHtml, e.ParentId, e.Roles, ie.ChildId, u.SamAccountName, u.Name, u.Title as UserTitle, u.ManagerId, u.Acadreorgid, u.DepartmentId, u.IsManager, u.Familieafdelingen, u.Department, i.Title as InstanceTitle, i.Modified, i.GraphId, i.SimulationId, e.Id AS TrueEventId, 
  CASE 
		WHEN CHARINDEX('[', e.EventId) > 0 AND CHARINDEX(']', e.eventid) = 
				LEN(e.EventId) THEN e.Title + SUBSTRING(e.eventid, CHARINDEX('[', e.EventId), LEN(e.eventid))
		ELSE e.Title
	END              AS EventTitle
  from [Event] as e INNER JOIN InstanceExtension ie ON e.InstanceId = ie.InstanceId INNER JOIN [User] as u ON e.Responsible = u.ID INNER JOIN Instance as i on i.Id = ie.InstanceId;