# defesas-ipt

Aplicação para marcar turnos para defesas de projetos.

## Setup

É necessário o .net Core SDK 2.2 para executar este projeto. Não é necessário nenhum motor de base de dados, já que a aplicação usa sqlite como base de dados.

É necessário também uma aplicação registada no portal do [Azure do IPT](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps).

Os valores devem ser configurados no ficheiro `appsettings.json` ou `User Secrets`.

Tal como outros projetos .net Core com Entity Framework, é necessário executar as migrações:

```bash
dotnet ef database update
```

Pode-se usar um programa como o [DBeaver](https://dbeaver.io/) para ver os conteúdos da base de dados.
