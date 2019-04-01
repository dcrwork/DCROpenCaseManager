ALTER TABLE InstanceExtension
	ADD ChildId INT;

ALTER TABLE InstanceExtension
	ADD FOREIGN KEY (ChildId) REFERENCES Child(Id);