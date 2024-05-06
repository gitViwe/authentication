namespace API.Extension;

public static class ApplicationBuilderExtension
{
    public static WebApplication CreateDatabaseTable(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<WebAuthenticationDbContext>();

        context.Database.Migrate();
        context.Database.ExecuteSqlRaw(
            """
            IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'CredentialRecords')
            BEGIN
                CREATE TABLE [dbo].[CredentialRecords] (
                    [Id]                        UNIQUEIDENTIFIER NOT NULL,
                    [RpId]                      NVARCHAR (256)   NOT NULL,
                    [UserHandle]                VARBINARY (128)  NOT NULL,
                    [CredentialId]              VARBINARY (1024) NOT NULL,
                    [Type]                      INT              NOT NULL,
                    [Kty]                       INT              NOT NULL,
                    [Alg]                       INT              NOT NULL,
                    [Ec2Crv]                    INT              NULL,
                    [Ec2X]                      VARBINARY (256)  NULL,
                    [Ec2Y]                      VARBINARY (256)  NULL,
                    [RsaModulusN]               VARBINARY (1024) NULL,
                    [RsaExponentE]              VARBINARY (32)   NULL,
                    [OkpCrv]                    INT              NULL,
                    [OkpX]                      VARBINARY (32)   NULL,
                    [SignCount]                 BIGINT           NOT NULL,
                    [Transports]                NVARCHAR (MAX)   NOT NULL,
                    [UvInitialized]             BIT              NOT NULL,
                    [BackupEligible]            BIT              NOT NULL,
                    [BackupState]               BIT              NOT NULL,
                    [AttestationObject]         VARBINARY (MAX)  NULL,
                    [AttestationClientDataJson] VARBINARY (MAX)  NULL,
                    [Description]               NVARCHAR (200)   NULL,
                    [CreatedAtUnixTime]         BIGINT           NOT NULL,
                    [UpdatedAtUnixTime]         BIGINT           NOT NULL
                );

                CREATE UNIQUE NONCLUSTERED INDEX [IX_CredentialRecords_RpId_UserHandle_CredentialId]
                    ON [dbo].[CredentialRecords]([RpId] ASC, [UserHandle] ASC, [CredentialId] ASC);

                ALTER TABLE [dbo].[CredentialRecords]
                    ADD CONSTRAINT [PK_CredentialRecords] PRIMARY KEY CLUSTERED ([Id] ASC);


                ALTER TABLE [dbo].[CredentialRecords]
                    ADD CONSTRAINT [Transports should be formatted as JSON] CHECK (isjson([Transports])=(1));

            END
            """);

        return app;
    }
}
