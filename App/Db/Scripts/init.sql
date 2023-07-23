--���� ����� � ���������� � �������� �� ����������, �� ����������� ������.
DECLARE @isExistsData INT
EXEC master.dbo.xp_fileexist '/data/shelters.csv', @isExistsData OUTPUT 
IF @isExistsData != 1 
BEGIN
	RETURN
END

--������� ��������� �������, ����� Shelters.
SELECT * INTO #SheltersTemp
FROM Shelters 
WHERE 1 <> 1

--��������� �� ����� �������� �� �������� �� ��������� �������.
BULK INSERT #SheltersTemp
FROM '/data/shelters.csv'
WITH
(
	FORMAT = 'csv',
    FIRSTROW = 1,
	DATAFILETYPE = 'widechar',
    FIELDTERMINATOR = '@',
    ROWTERMINATOR = '\n',
	TABLOCK
)

--������� ��������� ������� � ������� Shelters.
MERGE INTO Shelters
USING #SheltersTemp 
ON (
	Shelters.Id = #SheltersTemp.Id OR 
	Shelters.Address = #SheltersTemp.Address OR (
	Shelters.Latitude = Shelters.Latitude AND #SheltersTemp.Longitude = #SheltersTemp.Longitude
))
WHEN NOT MATCHED THEN 
INSERT (Id, City, Address, Description, Latitude, Longitude) VALUES (
	#SheltersTemp.Id, 
	#SheltersTemp.City, 
	#SheltersTemp.Address,
	#SheltersTemp.Description, 
	#SheltersTemp.Latitude, 
	#SheltersTemp.Longitude
);

--������� ��������� �������.
DELETE FROM #SheltersTemp