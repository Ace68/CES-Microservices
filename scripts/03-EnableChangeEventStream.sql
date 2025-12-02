CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'YourSuperecretPasword'

CREATE DATABASE SCOPED CREDENTIAL SqlCesCredential
WITH 
  IDENTITY = 'SHARED ACCESS SIGNATURE',
  SECRET = 'SAS-TOKEN-Created-From-01-Generate-SAS-Token.ps1'

-- Enable ChangeEventStream on the database
EXEC sys.sp_enable_event_stream

-- Verify ChangeEventStream is enabled on the database
SELECT * FROM sys.databases WHERE is_event_stream_enabled = 1
