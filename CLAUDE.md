# BaseLibrary: Visão Geral da Solução

## Estilo de escrita

- NUNCA use o travessão / em dash (`—`) em nenhum texto que você produzir: respostas no chat, comentários de código, mensagens de commit, documentação, este arquivo e os arquivos de memória. Use `:` ou `,` conforme couber. Preferência explícita do usuário.

## O que é esta solução

BaseLibrary é uma biblioteca multiuso geral em C# .NET 10, usada pelas demais soluções do usuário (em especial o Sindarin). Reúne utilitários de propósito amplo, sem depender do domínio de difração.

Componentes notáveis:

- **BaseLibrary.Math.Matrix** (namespace `Sindarin.Math.Matrix`): implementação própria de matrizes (Diagonal, Triangular, Sparse, Jagged3D, além das densas), que SUBSTITUIU o MathNet no `Sindarin.Objects.Calculation`. Inclui o caminho `Mᵀ·W·M` (equações normais) usado pelo NLS.
- **BaseLibrary.Math** e **BaseLibrary.Math.SpecialFunctions**: funções especiais próprias (Gamma, Erf/Erfc, Bessel, Struve), evitando dependência externa.
- **BaseLibrary.DependencyInjection**: padrão de injeção de dependência adotado nas soluções do usuário.
- **BaseLibrary.Console**: ferramentas de linha de comando reutilizáveis.
- **BaseLibrary.File**: serviços de arquivo (preferir `FileServices` a `FileMethods` quando houver as duas).

## Stack

- C# .NET 10
- Visual Studio + VSCode + GitHub

## Convenções

- Nomes de classes em PascalCase.
- Comentários de código podem estar em português; texto de usuário em inglês.
- Testes de unidade em xUnit + Moq + FluentAssertions, padrão AAA, um projeto de teste por projeto (ex.: `BaseLibrary.Tests`).
