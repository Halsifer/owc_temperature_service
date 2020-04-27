CREATE TABLE `temperatures` (
  `id` INT NOT NULL,
  `value` INT NULL,
  `created_date` DATETIME(6) NULL,
  `last_updated_date` DATETIME(6) NULL,
  `unit` VARCHAR(1) NULL,
  PRIMARY KEY (`id`));
