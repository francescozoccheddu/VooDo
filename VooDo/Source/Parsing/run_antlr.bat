@echo off
cd "%~dp0\Generated"
echo Building lexer...
call antlr4 "%~dp0\VooDoLexer.g4" -no-listener -visitor -o "%~dp0\Generated" -package "VooDo.Parsing.Generated" -Dlanguage=CSharp
echo Building parser...
call antlr4 "%~dp0\VooDoParser.g4" -no-listener -visitor -o "%~dp0\Generated" -package "VooDo.Parsing.Generated" -Dlanguage=CSharp
echo Done.