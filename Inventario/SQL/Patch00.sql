-- localhost\SQLEXPRESS01;LocalData
USE LocalData
GO
create schema Inventario;
go
/* ***** ***** *****  *****  USUARIO
**************************************************** */
if exists (select 1 from sys.tables where name ='Usuario') BEGIN
	DROP TABLE dbo.Usuario;
END 
GO
create table dbo.Usuario(
	IdUsuario int identity,
	Usuario varchar(20),
	Nombre varchar(150),
	FechaCreacion datetime not null default getdate(),
	CONSTRAINT PK_Usuario PRIMARY KEY (IdUsuario)
)
GO
insert into dbo.Usuario(Usuario, Nombre) values ('ST/david', 'David')
insert into dbo.Usuario(Usuario, Nombre) values ('ST/edgar', 'Edgar')
insert into dbo.Usuario(Usuario, Nombre) values ('ST/luis', 'Luis')
GO
/* ***** ***** *****  *****  EMPRESA
**************************************************** */
if exists (select 1 from sys.tables where name ='Empresa') BEGIN
	DROP TABLE dbo.Empresa;
END 
GO
create table dbo.Empresa(
	IdEmpresa int identity,
	Nombre varchar(150),
	FechaCreacion datetime not null default getdate(),
	CONSTRAINT PK_Empresa PRIMARY KEY (IdEmpresa)
)
GO
insert into dbo.Empresa(Nombre) values ('Bimbo')
insert into dbo.Empresa(Nombre) values ('Marinela')
GO
/* ***** *****  CATALOGO PRESENTACION
**************************************************** */
if exists (select 1 from sys.tables where name = 'CatPresentacion') BEGIN
	DROP TABLE Inventario.CatPresentacion
END
create table Inventario.CatPresentacion(
	IdPresentacion int identity,
	Nombre varchar(20),
	Visible bit default 1,
	FechaCreacion datetime default (getdate()),
	CONSTRAINT PK_Usuario PRIMARY KEY (IdPresentacion)
)
insert into Inventario.CatPresentacion(Nombre) values ('Tableta')
insert into Inventario.CatPresentacion(Nombre) values ('Frasco')
insert into Inventario.CatPresentacion(Nombre) values ('Sobre')
insert into Inventario.CatPresentacion(Nombre) values ('Inyección')
GO
/* ***** ***** ***** *****  MEDICAMENTO
**************************************************** */
if exists (select 1 from sys.tables where name ='Medicamento') BEGIN
	DROP TABLE Inventario.Medicamento;
END 
GO
create table Inventario.Medicamento(
	IdMedicamento int identity,
	Nombre varchar(120) not null,
	IdPresentacion int not null,
	Observaciones varchar(200),
	--MarcaPropia varchar(20),
	FechaCreacion datetime default(getdate()),
	IdUsuarioCreacion int not null,
	IdEmpresa int,
	CONSTRAINT PK_Medicamento_Nombre PRIMARY KEY (IdMedicamento),
	--CONSTRAINT PK_Medicamento PRIMARY KEY (
	--	IdMedicamento,
	--	IdEmpresa,
	--	IdPresentacion,
	--	Nombre
	--),
	CONSTRAINT FK_InvMedicamento_IdPresentacion FOREIGN KEY (IdPresentacion) REFERENCES Inventario.CatPresentacion(IdPresentacion),
	CONSTRAINT FK_InvMedicamento_IdUsuario FOREIGN KEY (IdUsuarioCreacion) REFERENCES Usuario(IdUsuario),
	CONSTRAINT FK_InvMedicamento_IdEmpresa FOREIGN KEY (IdEmpresa) REFERENCES Empresa(IdEmpresa)
	);
GO
/* ***** ***** ***** MOVIMIENTOS
**************************************************** */
if exists (select 1 from sys.tables where name ='Movimientos') BEGIN
	DROP TABLE Inventario.Movimientos;
END 
GO
create table Inventario.Movimientos(
	IdMovimiento int identity,
	IdMedicamento int not null,
	IdPresentacion int not null,
	Cantidad int,
	EsEntrada bit,
	FechaCreacion datetime default(getdate()),
	IdUsuarioCreacion int not null,
	IdEmpresa int NOT NULL,
	CONSTRAINT PK_Movimiento PRIMARY KEY (IdMovimiento),
	CONSTRAINT FK_InvMovimiento_IdMedicamento FOREIGN KEY (IdMedicamento) REFERENCES Inventario.Medicamento(IdMedicamento),
	CONSTRAINT FK_InvMovimiento_IdPresentacion FOREIGN KEY (IdPresentacion) REFERENCES Inventario.CatPresentacion(IdPresentacion),
	CONSTRAINT FK_InvMovimiento_IdUsuario FOREIGN KEY (IdUsuarioCreacion) REFERENCES Usuario(IdUsuario),
	CONSTRAINT FK_InvMovimiento_IdEmpresa FOREIGN KEY (IdEmpresa) REFERENCES Empresa(IdEmpresa)
)
go

/* ***** ***** ***** EXISTENCIA MEDICAMENTOS
**************************************************** */
if exists (select 1 from sys.tables where name ='Existencia') BEGIN
	DROP TABLE Inventario.Existencia;
END 
GO
create table Inventario.Existencia(
	IdMedicamento int NOT NULL,
	IdPresentacion int NOT NULL,
	Entradas int default 0,--Entradas decimal(12, 4) default 0,
	Salidas int default 0,--Salidas decimal(12, 4) default 0,
	--SaldoAnterior decimal(12, 4) NOT NULL,
	SaldoActual AS (CONVERT(int,Entradas+Salidas,(0))), --(CONVERT(decimal(12,4),Entradas+Salidas,(0))),	--(SaldoAnterior+Entradas)+Salidas,(0))),
	FechaCreacion datetime DEFAULT (getdate()),
	FechaModificacion datetime DEFAULT (getdate()),
	IdUsuarioCreacion int NOT NULL,
	IdEmpresa int NOT NULL,
	CONSTRAINT FK_InvExistencia_IdMedicamento FOREIGN KEY (IdMedicamento) REFERENCES Inventario.Medicamento(IdMedicamento),
	CONSTRAINT FK_InvExistencia_IdPresentacion FOREIGN KEY (IdPresentacion) REFERENCES Inventario.CatPresentacion(IdPresentacion),
	CONSTRAINT FK_InvExistencia_IdUsuario FOREIGN KEY (IdUsuarioCreacion) REFERENCES Usuario(IdUsuario),
	CONSTRAINT FK_InvExistencia_IdEmpresa FOREIGN KEY (IdEmpresa) REFERENCES Empresa(IdEmpresa)
)
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'EntradaSalida')
	DROP PROCEDURE Inventario.EntradaSalida
GO
create Procedure Inventario.EntradaSalida(
	@EsEntrada bit,
	@IdEmpresa int,
	@IdUsuario int,
	@Nombre varchar(120),
	@IdPresentacion int,
	@Observaciones varchar(200),
	@Cantidad int
)
AS BEGIN
	declare @IdMedicamento int, @ObservacionesArt varchar(200);
	-- INSERCIÓN DE ARTÍCULO.
	if not exists(select 1 from Inventario.Medicamento where IdEmpresa = @IdEmpresa and Nombre = @Nombre and IdPresentacion = @IdPresentacion)
	BEGIN
		insert into Inventario.Medicamento(IdEmpresa, Nombre, IdPresentacion, Observaciones, IdUsuarioCreacion)
			values(@IdEmpresa, @Nombre, @IdPresentacion, @Observaciones, @IdUsuario)
		select @IdMedicamento = SCOPE_IDENTITY();
	END
	ELSE BEGIN
		select @IdMedicamento = IdMedicamento, @ObservacionesArt = Observaciones from Inventario.Medicamento where IdEmpresa = @IdEmpresa and Nombre = @Nombre and IdPresentacion = @IdPresentacion
		if @Observaciones <> @ObservacionesArt
			update Inventario.Medicamento set Observaciones = @Observaciones where IdMedicamento = @IdMedicamento
	END
	-- INSERCIÓN DE MOVIMIENTO.
	insert into Inventario.Movimientos(IdMedicamento, IdPresentacion, Cantidad, IdUsuarioCreacion, IdEmpresa, EsEntrada)
		values(@IdMedicamento, @IdPresentacion, (case when @EsEntrada = 1 then @Cantidad else @Cantidad * (-1) end), @IdUsuario, @IdEmpresa, @EsEntrada)
	-- INSERCIÓN O ACTUALIZACIÓN DE LA CANTIDAD DE ARTÍCULOS.
	IF not exists(select 1 from Inventario.Existencia where IdMedicamento = @IdMedicamento and IdEmpresa = @IdEmpresa and IdPresentacion = @IdPresentacion)
	BEGIN
		if @EsEntrada = 1
			insert into Inventario.Existencia(IdMedicamento, IdEmpresa, IdPresentacion, IdUsuarioCreacion, Entradas)
				values(@IdMedicamento, @IdEmpresa, @IdPresentacion, @IdUsuario, @Cantidad)
		else
			insert into Inventario.Existencia(IdMedicamento, IdEmpresa, IdPresentacion, IdUsuarioCreacion, Salidas)
				values(@IdMedicamento, @IdEmpresa, @IdPresentacion, @IdUsuario, @Cantidad * (-1))
	END
	ELSE BEGIN
		if @EsEntrada = 1
			update Inventario.Existencia set Entradas = Entradas + @Cantidad, IdUsuarioCreacion = @IdUsuario 
				where IdMedicamento = @IdMedicamento and IdEmpresa = @IdEmpresa and IdPresentacion = @IdPresentacion
		else
			update Inventario.Existencia set Salidas = Salidas + (@Cantidad * (-1)), IdUsuarioCreacion = @IdUsuario 
				where IdMedicamento = @IdMedicamento and IdEmpresa = @IdEmpresa and IdPresentacion = @IdPresentacion
	END
	--exec Inventario.EntradaSalida 1, 1, 77, 'PARACETAMOL', 1, '', 2
END
GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ActualizaExistencia')
	DROP PROCEDURE Inventario.ActualizaExistencia
GO
create Procedure Inventario.ActualizaExistencia(
	@IdEmpresa int = -1,
	@Nombre varchar(120) = '-1',
	@IdPresentacion int = -1
)
AS BEGIN
	declare @IdMedicamento int = -1
	if @Nombre <> '-1'
		select @IdMedicamento = IdMedicamento from Inventario.Medicamento where Nombre = @Nombre and @IdEmpresa = IdEmpresa and @IdPresentacion = IdPresentacion
		
	select	IdEmpresa,
			IdMedicamento,
			IdPresentacion,
			SUM(case when Cantidad < 0 then Cantidad else 0 end) salidas, 
			SUM(case when Cantidad > 0 then Cantidad else 0 end) entradas
	into #tempMed
	from(
			select 
				IdEmpresa,
				IdMedicamento,
				IdPresentacion,
				Cantidad
			from
				Inventario.Movimientos mov
			where @idMedicamento in ('-1',mov.idMedicamento)
		) a
	GROUP by IdEmpresa, IdMedicamento, IdPresentacion

	update ie
	set Entradas = t.Entradas,
		Salidas = t.Salidas
	from Inventario.Existencia ie
		inner join #tempMed t on t.idempresa = ie.IdEmpresa and t.IdMedicamento = ie.IdMedicamento and t.IdPresentacion = ie.IdPresentacion
	where @IdMedicamento in ('-1',ie.IdMedicamento) and @IdEmpresa in ('-1',ie.IdEmpresa) and @IdPresentacion in ('-1',ie.IdPresentacion)
	drop table #tempMed
END
GO

/* LIMPIEZA DE INVENTARIO.

drop table Inventario.Existencia
drop table Inventario.Movimientos
drop table Inventario.Medicamento
drop table Inventario.CatPresentacion
drop table dbo.Empresa
drop table dbo.Usuario
DROP PROCEDURE Inventario.ActualizaExistencia
DROP PROCEDURE Inventario.EntradaSalida
drop schema Inventario

---*/