/*
    Vintaloo - Esquema de base de datos SQL Server
    Generado desde vintaloo_db. Ejecutar sobre una base vacia.
    Incluye tablas, restricciones, indices, claves foraneas, datos de roles y stored procedures.
*/

IF DB_ID(N'vintaloo_db') IS NULL
BEGIN
    CREATE DATABASE [vintaloo_db];
END
GO

USE [vintaloo_db];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* Tabla: dbo.usuarios */
CREATE TABLE [dbo].[usuarios] (
    [id_usuario] int IDENTITY(1,1) NOT NULL,
    [nombre] varchar(100) NOT NULL,
    [correo] varchar(150) NOT NULL,
    [password_hash] varchar(255) NOT NULL,
    [telefono] varchar(20) NULL,
    [ubicacion] varchar(150) NULL,
    [foto_perfil] varchar(255) NULL,
    [fecha_registro] datetime NULL CONSTRAINT [DF__usuarios__fecha___4AB81AF0] DEFAULT (getdate()),
    [estado] varchar(20) NULL CONSTRAINT [DF__usuarios__estado__4BAC3F29] DEFAULT ('activo'),
    CONSTRAINT [PK__usuarios__4E3E04ADE6F27E8B] PRIMARY KEY ([id_usuario]),
    CONSTRAINT [UQ__usuarios__2A586E0BE35A39B8] UNIQUE ([correo]),
    CONSTRAINT [CK__usuarios__estado__4CA06362] CHECK ([estado]='bloqueado' OR [estado]='activo')
);
GO

/* Tabla: dbo.roles */
CREATE TABLE [dbo].[roles] (
    [id_rol] int IDENTITY(1,1) NOT NULL,
    [nombre] varchar(50) NOT NULL,
    CONSTRAINT [PK__roles__6ABCB5E062489A64] PRIMARY KEY ([id_rol]),
    CONSTRAINT [UQ__roles__72AFBCC641404E66] UNIQUE ([nombre])
);
GO

/* Tabla: dbo.categorias */
CREATE TABLE [dbo].[categorias] (
    [id_categoria] int IDENTITY(1,1) NOT NULL,
    [nombre] varchar(100) NOT NULL,
    [descripcion] varchar(150) NULL,
    CONSTRAINT [PK__categori__CD54BC5A8AE21DAA] PRIMARY KEY ([id_categoria])
);
GO

/* Tabla: dbo.articulos */
CREATE TABLE [dbo].[articulos] (
    [id_articulo] int IDENTITY(1,1) NOT NULL,
    [id_usuario] int NOT NULL,
    [id_categoria] int NOT NULL,
    [titulo] varchar(150) NOT NULL,
    [descripcion] varchar(max) NOT NULL,
    [precio] decimal(10,2) NOT NULL,
    [estado_producto] varchar(20) NOT NULL,
    [estado_publicacion] varchar(20) NULL CONSTRAINT [DF__articulos__estad__534D60F1] DEFAULT ('disponible'),
    [ubicacion] varchar(150) NULL,
    [fecha_publicacion] datetime NULL CONSTRAINT [DF__articulos__fecha__5535A963] DEFAULT (getdate()),
    CONSTRAINT [PK__articulo__3F6E82880360A385] PRIMARY KEY ([id_articulo]),
    CONSTRAINT [CK__articulos__estad__5441852A] CHECK ([estado_publicacion]='vendido' OR [estado_publicacion]='reservado' OR [estado_publicacion]='disponible'),
    CONSTRAINT [CK_articulos_estado_producto] CHECK ([estado_producto]='regular' OR [estado_producto]='bueno' OR [estado_producto]='excelente')
);
GO

/* Tabla: dbo.imagenes_articulos */
CREATE TABLE [dbo].[imagenes_articulos] (
    [id_imagen] int IDENTITY(1,1) NOT NULL,
    [id_articulo] int NOT NULL,
    [url_imagen] varchar(255) NOT NULL,
    CONSTRAINT [PK__imagenes__27CC26896C89CDED] PRIMARY KEY ([id_imagen])
);
GO

/* Tabla: dbo.favoritos */
CREATE TABLE [dbo].[favoritos] (
    [id_favorito] int IDENTITY(1,1) NOT NULL,
    [id_usuario] int NOT NULL,
    [id_articulo] int NOT NULL,
    [fecha_agregado] datetime NULL CONSTRAINT [DF__favoritos__fecha__5DCAEF64] DEFAULT (getdate()),
    CONSTRAINT [PK__favorito__78F875AE178EEBF9] PRIMARY KEY ([id_favorito]),
    CONSTRAINT [UQ_favoritos] UNIQUE ([id_usuario], [id_articulo])
);
GO

/* Tabla: dbo.reportes */
CREATE TABLE [dbo].[reportes] (
    [id_reporte] int IDENTITY(1,1) NOT NULL,
    [id_usuario] int NOT NULL,
    [id_articulo] int NOT NULL,
    [motivo] varchar(255) NOT NULL,
    [descripcion] varchar(max) NULL,
    [fecha_reporte] datetime NULL CONSTRAINT [DF__reportes__fecha___628FA481] DEFAULT (getdate()),
    [estado] varchar(20) NULL CONSTRAINT [DF__reportes__estado__6383C8BA] DEFAULT ('pendiente'),
    CONSTRAINT [PK__reportes__87E4F5CBBD48DA17] PRIMARY KEY ([id_reporte]),
    CONSTRAINT [CK__reportes__estado__6477ECF3] CHECK ([estado]='rechazado' OR [estado]='revisado' OR [estado]='pendiente')
);
GO

/* Tabla: dbo.usuario_roles */
CREATE TABLE [dbo].[usuario_roles] (
    [id_usuario] int NOT NULL,
    [id_rol] int NOT NULL,
    CONSTRAINT [PK__usuario___5895CFF3B07353EC] PRIMARY KEY ([id_usuario], [id_rol])
);
GO

ALTER TABLE [dbo].[articulos] WITH CHECK ADD CONSTRAINT [FK__articulos__id_ca__571DF1D5] FOREIGN KEY ([id_categoria]) REFERENCES [dbo].[categorias] ([id_categoria]);
ALTER TABLE [dbo].[articulos] CHECK CONSTRAINT [FK__articulos__id_ca__571DF1D5];
GO
ALTER TABLE [dbo].[articulos] WITH CHECK ADD CONSTRAINT [FK__articulos__id_us__5629CD9C] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[usuarios] ([id_usuario]);
ALTER TABLE [dbo].[articulos] CHECK CONSTRAINT [FK__articulos__id_us__5629CD9C];
GO
ALTER TABLE [dbo].[favoritos] WITH CHECK ADD CONSTRAINT [FK__favoritos__id_ar__5FB337D6] FOREIGN KEY ([id_articulo]) REFERENCES [dbo].[articulos] ([id_articulo]) ON DELETE CASCADE;
ALTER TABLE [dbo].[favoritos] CHECK CONSTRAINT [FK__favoritos__id_ar__5FB337D6];
GO
ALTER TABLE [dbo].[favoritos] WITH CHECK ADD CONSTRAINT [FK__favoritos__id_us__5EBF139D] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[usuarios] ([id_usuario]) ON DELETE CASCADE;
ALTER TABLE [dbo].[favoritos] CHECK CONSTRAINT [FK__favoritos__id_us__5EBF139D];
GO
ALTER TABLE [dbo].[imagenes_articulos] WITH CHECK ADD CONSTRAINT [FK__imagenes___id_ar__59FA5E80] FOREIGN KEY ([id_articulo]) REFERENCES [dbo].[articulos] ([id_articulo]) ON DELETE CASCADE;
ALTER TABLE [dbo].[imagenes_articulos] CHECK CONSTRAINT [FK__imagenes___id_ar__59FA5E80];
GO
ALTER TABLE [dbo].[reportes] WITH CHECK ADD CONSTRAINT [FK__reportes__id_art__66603565] FOREIGN KEY ([id_articulo]) REFERENCES [dbo].[articulos] ([id_articulo]);
ALTER TABLE [dbo].[reportes] CHECK CONSTRAINT [FK__reportes__id_art__66603565];
GO
ALTER TABLE [dbo].[reportes] WITH CHECK ADD CONSTRAINT [FK__reportes__id_usu__656C112C] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[usuarios] ([id_usuario]);
ALTER TABLE [dbo].[reportes] CHECK CONSTRAINT [FK__reportes__id_usu__656C112C];
GO
ALTER TABLE [dbo].[usuario_roles] WITH CHECK ADD CONSTRAINT [FK__usuario_r__id_ro__662B2B3B] FOREIGN KEY ([id_rol]) REFERENCES [dbo].[roles] ([id_rol]) ON DELETE CASCADE;
ALTER TABLE [dbo].[usuario_roles] CHECK CONSTRAINT [FK__usuario_r__id_ro__662B2B3B];
GO
ALTER TABLE [dbo].[usuario_roles] WITH CHECK ADD CONSTRAINT [FK__usuario_r__id_us__65370702] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[usuarios] ([id_usuario]) ON DELETE CASCADE;
ALTER TABLE [dbo].[usuario_roles] CHECK CONSTRAINT [FK__usuario_r__id_us__65370702];
GO


/* Datos base: roles */
SET IDENTITY_INSERT [dbo].[roles] ON;
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[roles] WHERE [id_rol] = 1) INSERT [dbo].[roles] ([id_rol], [nombre]) VALUES (1, N'admin');
IF NOT EXISTS (SELECT 1 FROM [dbo].[roles] WHERE [id_rol] = 2) INSERT [dbo].[roles] ([id_rol], [nombre]) VALUES (2, N'usuario');
GO
SET IDENTITY_INSERT [dbo].[roles] OFF;
GO

/* Stored procedure: dbo.sp_actualizar_categoria */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_actualizar_categoria 
    @id_categoria INT,
    @nombre VARCHAR(100),
    @descripcion VARCHAR(150)
AS
BEGIN 
	UPDATE categorias
	SET nombre = @nombre, 
		descripcion = @descripcion
	WHERE id_categoria = @id_categoria;
END

GO

/* Stored procedure: dbo.sp_actualizar_usuario */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_actualizar_usuario
    @id_usuario INT,
    @nombre VARCHAR(100),
    @telefono VARCHAR(20),
    @ubicacion VARCHAR(150),
    @foto_perfil VARCHAR(255),
    @estado VARCHAR(20)
AS
BEGIN
	UPDATE usuarios 
	SET nombre = @nombre, 
		telefono = @telefono,
		ubicacion = @ubicacion,
		foto_perfil = @foto_perfil, 
		estado = @estado
	WHERE id_usuario = @id_usuario;
END

GO

/* Stored procedure: dbo.sp_agregar_fav */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_agregar_fav
    @id_usuario INT,
    @id_articulo INT
AS
BEGIN
	INSERT INTO favoritos (id_usuario, id_articulo)
	VALUES (@id_usuario, @id_articulo); 
END

GO

/* Stored procedure: dbo.sp_articulo_porId */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_articulo_porId
    @id_articulo INT
AS
BEGIN
    SELECT 
        a.*,
        c.nombre AS Categoria,
        (
            SELECT TOP 1 i.url_imagen
            FROM imagenes_articulos i
            WHERE i.id_articulo = a.id_articulo
        ) AS UrlImagen
    FROM articulos a
    INNER JOIN categorias c ON a.id_categoria = c.id_categoria
    WHERE a.id_articulo = @id_articulo
END
GO

/* Stored procedure: dbo.sp_cambiar_estado_articulo */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_cambiar_estado_articulo
    @id_articulo INT,
    @estado_publicacion VARCHAR(20)
AS
BEGIN
    UPDATE articulos
    SET estado_publicacion = @estado_publicacion
    WHERE id_articulo = @id_articulo;
END

GO

/* Stored procedure: dbo.sp_crear_reporte */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_crear_reporte
    @id_usuario INT,
    @id_articulo INT,
    @motivo VARCHAR(255),
    @descripcion VARCHAR(MAX)
AS
BEGIN
    INSERT INTO reportes
    (id_usuario, id_articulo, motivo, descripcion)
    VALUES
    (@id_usuario, @id_articulo, @motivo, @descripcion);
END

GO

/* Stored procedure: dbo.sp_eliminar_articulo */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_eliminar_articulo
    @id_articulo INT
AS
BEGIN
    DELETE FROM articulos
    WHERE id_articulo = @id_articulo;
END

GO

/* Stored procedure: dbo.sp_eliminar_categoria */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_eliminar_categoria
	@id_categoria INT
AS
BEGIN
	DELETE categorias
	WHERE id_categoria = @id_categoria; 
END

GO

/* Stored procedure: dbo.sp_eliminar_favorito */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_eliminar_favorito
    @id_usuario INT,
    @id_articulo INT
AS
BEGIN
    DELETE FROM favoritos
    WHERE id_usuario = @id_usuario 
    AND id_articulo = @id_articulo;
END

GO

/* Stored procedure: dbo.sp_eliminar_usuario */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_eliminar_usuario
	@id_usuario INT 
AS
BEGIN
	DELETE FROM usuarios
	WHERE id_usuario = @id_usuario;
END 

GO

/* Stored procedure: dbo.sp_favoritos_usuario */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_favoritos_usuario
    @id_usuario INT
AS
BEGIN
    SELECT a.*
    FROM favoritos f
    INNER JOIN articulos a 
        ON f.id_articulo = a.id_articulo
    WHERE f.id_usuario = @id_usuario;
END

GO

/* Stored procedure: dbo.sp_insertar_articulo */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_insertar_articulo
    @id_usuario      INT,
    @id_categoria    INT,
    @titulo          VARCHAR(150),
    @descripcion     VARCHAR(MAX),
    @precio          DECIMAL(10,2),
    @estado_producto VARCHAR(20),
    @ubicacion       VARCHAR(150)
AS
BEGIN
    INSERT INTO articulos 
        (id_usuario, id_categoria, titulo, descripcion, precio, estado_producto, ubicacion)
    VALUES 
        (@id_usuario, @id_categoria, @titulo, @descripcion, @precio, @estado_producto, @ubicacion)

    -- Retorna el ID recién insertado
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS IdArticulo;
END
GO

/* Stored procedure: dbo.sp_insertar_categoria */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_insertar_categoria
    @nombre VARCHAR(100),
    @descripcion VARCHAR(150)
AS
BEGIN
	INSERT INTO categorias (nombre, descripcion)
	VALUES (@nombre, @descripcion);
END

GO

/* Stored procedure: dbo.sp_insertar_imagen_articulo */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_insertar_imagen_articulo
    @id_articulo INT,
    @url_imagen VARCHAR(255)
AS
BEGIN
    INSERT INTO imagenes_articulos (id_articulo, url_imagen)
    VALUES (@id_articulo, @url_imagen);
END

GO

/* Stored procedure: dbo.sp_insertar_usuario */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_insertar_usuario
    @nombre VARCHAR(100),
    @correo VARCHAR(150),
    @password_hash VARCHAR(255),
    @telefono VARCHAR(20),
    @ubicacion VARCHAR(150),
    @foto_perfil VARCHAR(255)
AS
BEGIN
    INSERT INTO usuarios
    (nombre, correo, password_hash, telefono, ubicacion, foto_perfil)
    VALUES
    (@nombre, @correo, @password_hash, @telefono, @ubicacion, @foto_perfil);
END

GO

/* Stored procedure: dbo.sp_login_usuario */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE sp_login_usuario
	@correo VARCHAR(150)
AS
BEGIN
	SELECT * 
	FROM usuarios
	WHERE correo = @correo;
END 

GO

/* Stored procedure: dbo.sp_obtener_articulos */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_obtener_articulos
AS
BEGIN
    SELECT 
        id_articulo,
        id_usuario,
        id_categoria,
        titulo,
        descripcion,
        precio,
        estado_producto,
        estado_publicacion,
        ubicacion,
        fecha_publicacion
    FROM articulos
END
GO

/* Stored procedure: dbo.sp_obtener_categoria */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_obtener_categoria
AS
BEGIN 
	SELECT * FROM categorias; 
END

GO

/* Stored procedure: dbo.sp_obtener_imagenes_articulo */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_obtener_imagenes_articulo
    @id_articulo INT
AS
BEGIN
    SELECT * 
    FROM imagenes_articulos
    WHERE id_articulo = @id_articulo;
END

GO

/* Stored procedure: dbo.sp_obtener_reportes */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_obtener_reportes
AS
BEGIN
    SELECT 
        r.*,
        u.nombre AS usuario,
        a.titulo AS articulo
    FROM reportes r
    INNER JOIN usuarios u ON r.id_usuario = u.id_usuario
    INNER JOIN articulos a ON r.id_articulo = a.id_articulo;
END

GO

/* Stored procedure: dbo.sp_obtener_usuarios */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_obtener_usuarios
AS
BEGIN
	Select * from usuarios; 
END 

GO

/* Stored procedure: dbo.sp_obtener_usuarios_porId */
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE PROCEDURE sp_obtener_usuarios_porId
	@id_usuario INT
AS
BEGIN
	SELECT * FROM usuarios 
	WHERE id_usuario = @id_usuario; 
END

GO

