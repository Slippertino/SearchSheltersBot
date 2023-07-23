--Если файла с сведениями о убежищах не существует, то заканчиваем скрипт.
DECLARE @isExistsData INT
EXEC master.dbo.xp_fileexist '/data/shelters.csv', @isExistsData OUTPUT 
IF @isExistsData != 1 
BEGIN
	RETURN
END

--Создаем временную таблицу, копию Shelters.
SELECT * INTO #SheltersTemp
FROM Shelters 
WHERE 1 <> 1

--Загружаем из файла сведения об убежищах во временную таблицу.
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

--Сливаем временную таблицу в таблицу Shelters.
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

--Очищаем временную таблицу.
DELETE FROM #SheltersTemp