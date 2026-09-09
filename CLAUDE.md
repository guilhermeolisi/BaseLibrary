# BaseLibrary: biblioteca multiuso

Biblioteca C# .NET 10 usada pelas demais solucoes do workspace (Sindarin, Nimloth,
SindarinAI, AutoUpdater, GOS*). Utilitarios de proposito amplo, sem depender do
dominio de difracao. Regras compartilhadas em `../CLAUDE.md`.

E dependencia de 14 projetos do `Nimloth.sln` e 12 do `Sindarin.sln`: mudanca de
API aqui quebra consumidores. Commite BaseLibrary ANTES dos consumidores e rode o
build deles (`dotnet build ../Nimloth/Nimloth.sln`) depois de mudar assinatura.

## Componentes notaveis

- **BaseLibrary.Math.Matrix** (namespace `Sindarin.Math.Matrix`): matrizes proprias
  (Diagonal, Triangular, Sparse, Jagged3D, densas). SUBSTITUIU o MathNet no
  `Sindarin.Objects.Calculation`; inclui o caminho `M^T W M` (equacoes normais) do NLS.
- **BaseLibrary.Math** e **BaseLibrary.Math.SpecialFunctions**: Gamma, Erf/Erfc,
  Bessel, Struve, sem dependencia externa.
- **BaseLibrary.DependencyInjection**: padrao de DI de todas as solucoes, exceto o
  nenhuma: o Nimloth tambem usa este, apesar do que o texto antigo dizia (medido em 08/09/2026).
- **BaseLibrary.Console**: ferramentas de linha de comando reutilizaveis.
- **BaseLibrary.File**: preferir `FileServices` a `FileMethods` quando houver as duas.
- Tambem: General, Collections, Exception, HTTP, Text, Numbers.

## Convencoes

- Classes em PascalCase. Comentarios podem ser em portugues; texto de usuario em ingles.
- Preferir classe instanciavel com interface a API estatica (permite DI e mock).
- Testes: xUnit + Moq + FluentAssertions, AAA, um projeto de teste por projeto
  (`BaseLibrary.Tests`). Regras completas em `../.claude/rules/testes.md`.

## Comandos

- `dotnet build BaseLibrary.sln --no-restore`
- `dotnet test BaseLibrary.sln --no-restore`
