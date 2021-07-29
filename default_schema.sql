DROP TABLE IF EXISTS `Users`;
DROP TABLE IF EXISTS `Groups`;
DROP TABLE IF EXISTS `Cluster`;

CREATE TABLE `Groups` (
	`Name` VARCHAR(20) NOT NULL UNIQUE,
	`Permissions` VARCHAR(255),
	PRIMARY KEY (`Name`)
);

CREATE TABLE Users (
	`SteamID` VARCHAR(20) NOT NULL UNIQUE,
	`Name` VARCHAR(20) NOT NULL UNIQUE,
	`GroupName` VARCHAR(20) NOT NULL,
	`AdditionalPermissions` VARCHAR(255),
	`Banned` BIT NOT NULL DEFAULT (0),
	`LastPlayed` DateTime NOT NULL DEFAULT NOW(),

	PRIMARY KEY (`SteamID`),
	FOREIGN KEY (`GroupName`) REFERENCES `Groups`(`Name`)
);

CREATE TABLE Cluster (
	`Name` VARCHAR(20) NOT NULL UNIQUE,
	`IP` VARCHAR(20) NOT NULL,
	`Port` VARCHAR(5),
	`Votes` int DEFAULT(0),

	PRIMARY KEY (`Name`)
);

INSERT INTO `Groups` (Name, Permissions)
VALUES ('Admin', 'all');

INSERT INTO `Groups` (Name, Permissions)
VALUES ('User', '');

INSERT INTO `Users` (SteamID, Name, GroupName)
VALUES (0, 'Admin', 'Admin');

INSERT INTO `Cluster` (Name, IP, Port)
VALUES ('X', '0.0.0.0', '27015');