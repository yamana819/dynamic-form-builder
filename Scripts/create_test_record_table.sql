USE DynamicFormBuilderDB;
GO

CREATE TABLE kayit_test (
    kayit_kodu NVARCHAR(15),
    isim NVARCHAR(20) NOT NULL,
    soyisim NVARCHAR(25) NOT NULL,
    dogum_tarihi DATETIME NOT NULL,
    adres NVARCHAR(70),
    is_deleted BIT NOT NULL DEFAULT 0
);
GO

TRUNCATE TABLE kayit_test;
GO

ALTER TABLE kayit_test 
DROP COLUMN kayit_kodu;
GO

ALTER TABLE kayit_test
ADD kayit_kodu NVARCHAR(15) PRIMARY KEY;
GO


DROP TABLE kayıt_test;
GO

CREATE VIEW vw_kayit_test AS
SELECT 
    kayit_kodu,                  
    isim AS 'İsim',
    soyisim AS 'Soyisim',
    dogum_tarihi AS 'Doğum Tarihi',
    adres AS 'Adres'
FROM 
    kayit_test
WHERE 
    is_deleted = 0;
GO