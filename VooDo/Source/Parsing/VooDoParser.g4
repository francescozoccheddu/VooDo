parser grammar VooDoParser
;

options
{
	tokenVocab = VooDoLexer;
}

// Entry point

script
	: extern_alias_directive* using_directive* globals_block? statement* EOF
;

globals_block
	: GLOBALS OPEN_BRACE implicit_global_declaration_statement* CLOSE_BRACE
;

// Basic concepts

namespace_or_type_name
	: ( identifier type_argument_list? | qualified_alias_member) ( DOT identifier type_argument_list?)*
;

// Types

type
	: base_type (rank_specifier)*
;

base_type
	: simple_or_class_type | tuple_type
;

simple_or_class_type
	: predefined_struct_type | class_type
;

tuple_type
	: OPEN_PARENS tuple_element ( COMMA tuple_element)+ CLOSE_PARENS
;

tuple_element
	: type identifier?
;

predefined_struct_type
	: SBYTE | BYTE | SHORT | USHORT | INT | UINT | LONG | ULONG | CHAR | DECIMAL | FLOAT | DOUBLE | BOOL
;

predefined_class_type
	: OBJECT | STRING
;

predefined_type
	: predefined_class_type | predefined_struct_type
;

class_type
	: namespace_or_type_name | predefined_class_type
;

type_argument_list
	: LT type ( COMMA type)* GT
;

// Expressions

argument_list
	: argument ( COMMA argument)*
;

argument
	: (identifier COLON)? kind = (REF | IN)? expression | (identifier COLON)? kind = OUT type_or_var? expression
;

unbound_type_name
	: identifier (generic_dimension_specifier? | DCOLON identifier generic_dimension_specifier?)
	(
		DOT identifier generic_dimension_specifier?
	)*
;

generic_dimension_specifier
	: LT COMMA* GT
;

anonymous_object_initializer
	: OPEN_BRACE (member_declarator_list COMMA?)? CLOSE_BRACE
;

member_declarator_list
	: member_declarator ( COMMA member_declarator)*
;

member_declarator
	: member_access_expression | identifier | identifier ASSIGN value_expression
;

expression
	: ref = REF? value_expression
;

bracket_expression
	: INTERR? OPEN_BRACKET indexer_argument ( COMMA indexer_argument)* CLOSE_BRACKET
;

indexer_argument
	: (identifier COLON)? expression
;

value_expression
	: literal
	| CONTROLLEROF identifier
	| identifier type_argument_list?
	| OPEN_PARENS value_expression CLOSE_PARENS
	| NEW
	(
		type
		(
			object_creation_expression
			| object_or_collection_initializer
			| OPEN_BRACKET value_expression (COMMA value_expression)* CLOSE_BRACKET rank_specifier* array_initializer?
			| rank_specifier+ array_initializer
		)
		| anonymous_object_initializer
		| rank_specifier array_initializer
	)
	| OPEN_PARENS expression ( COMMA expression)+ CLOSE_PARENS
	| TYPEOF OPEN_PARENS (unbound_type_name | type | VOID) CLOSE_PARENS
	| DEFAULT (OPEN_PARENS type CLOSE_PARENS)?
	| NAMEOF OPEN_PARENS (identifier DOT)* identifier CLOSE_PARENS
	| value_expression INTERR? DOT identifier type_argument_list?
	| predefined_type INTERR? DOT identifier type_argument_list?
	| qualified_alias_member INTERR? DOT identifier type_argument_list?
	| value_expression OPEN_PARENS argument_list? CLOSE_PARENS
	| value_expression bracket_expression+
	| PLUS value_expression
	| MINUS value_expression
	| NOT value_expression
	| BCOMPL value_expression
	| OPEN_PARENS type CLOSE_PARENS value_expression
	| AND value_expression
	| OR value_expression
	| value_expression (MUL | DIV | MOD) value_expression
	| value_expression (PLUS | MINUS) value_expression
	| value_expression (LSH | RSH) value_expression
	| value_expression (LT | GT | LE | GE) value_expression
	| value_expression (EQ | NEQ) value_expression
	| value_expression AND value_expression
	| value_expression XOR value_expression
	| value_expression OR value_expression
	| value_expression LAND value_expression
	| value_expression LOR value_expression
	| value_expression NULLC value_expression_or_throw
	| value_expression INTERR expression_or_throw COLON expression_or_throw
;

member_access_expression
	:  value_expression INTERR? DOT identifier type_argument_list?
	| predefined_type INTERR? DOT identifier type_argument_list?
	| qualified_alias_member INTERR? DOT identifier type_argument_list?
;

expression_or_throw
	: expression | throw_expression
;

value_expression_or_throw
	: value_expression | throw_expression
;

throw_expression
	: THROW value_expression
;

object_or_collection_initializer
	: object_initializer | collection_initializer
;

object_initializer
	: OPEN_BRACE (member_initializer_list COMMA?)? CLOSE_BRACE
;

member_initializer_list
	: member_initializer (COMMA member_initializer)*
;

member_initializer
	: (identifier | OPEN_BRACKET expression CLOSE_BRACKET) ASSIGN initializer_value // C# 6
;

initializer_value
	: expression | object_or_collection_initializer
;

collection_initializer
	: OPEN_BRACE element_initializer (COMMA element_initializer)* COMMA? CLOSE_BRACE
;

element_initializer
	: value_expression | OPEN_BRACE value_expression (COMMA value_expression)* CLOSE_BRACE
;

object_creation_expression
	: OPEN_PARENS argument_list? CLOSE_PARENS object_or_collection_initializer?
;

// Arrays
rank_specifier
	: OPEN_BRACKET COMMA* CLOSE_BRACKET
;

array_initializer
	: OPEN_BRACE (variable_initializer (COMMA variable_initializer)* COMMA?)? CLOSE_BRACE
;

// Statements

block
	: OPEN_BRACE statement* CLOSE_BRACE
;

statement
	: value_expression SEMICOLON
	| value_expression
	(
		ASSIGN
		| ASSIGN_ADD
		| ASSIGN_AND
		| ASSIGN_DIV
		| ASSIGN_LSH
		| ASSIGN_MOD
		| ASSIGN_MUL
		| ASSIGN_NULLC
		| ASSIGN_OR
		| ASSIGN_RSH
		| ASSIGN_SUB
		| ASSIGN_XOR
	) expression
	| declaration_statement
	| IF OPEN_PARENS expression CLOSE_PARENS statement (ELSE statement)?
	| RETURN expression SEMICOLON
	| THROW value_expression? SEMICOLON
	| TRY block (catch_clauses finally_clause? | finally_clause)
	| USING OPEN_PARENS (variable_declaration | value_expression) CLOSE_PARENS statement
;

catch_clauses
	: specific_catch_clause (specific_catch_clause)* general_catch_clause? | general_catch_clause
;

specific_catch_clause
	: CATCH OPEN_PARENS class_type identifier? CLOSE_PARENS block
;

general_catch_clause
	: CATCH block
;

finally_clause
	: FINALLY block
;

declaration_statement
	: variable_declaration SEMICOLON | global_declaration SEMICOLON | constant_declaration SEMICOLON
;

implicit_global_declaration_statement
	: implicit_global_declaration SEMICOLON
;

type_or_var
	: type | VAR
;

constant_declarator
	: identifier ASSIGN expression
;

variable_declarator
	: identifier (ASSIGN REF? variable_initializer)?
;

global_declarator
	: identifier (ASSIGN variable_initializer)?
;

variable_initializer
	: expression | array_initializer
;

constant_declaration
	: CONST type_or_var constant_declarator (COMMA constant_declarator)*
;

variable_declaration
	: (USING | REF | REF READONLY)? type_or_var variable_declarator ( COMMA variable_declarator)*
;

global_declaration
	: GLOBAL implicit_global_declaration
;

implicit_global_declaration
	: type_or_var global_declarator ( COMMA global_declarator)*
;

// Namespaces

extern_alias_directive
	: EXTERN ALIAS identifier SEMICOLON
;

using_directive
	: USING identifier SEMICOLON					# UsingAlias
	| USING namespace_or_type_name SEMICOLON		# UsingNamespace
	| USING STATIC namespace_or_type_name SEMICOLON	# UsingStatic
;

qualified_alias_member
	: identifier DCOLON identifier type_argument_list?
;

literal
	: boolean_literal
	| string_literal
	| DEC_INTEGER_LITERAL
	| HEX_INTEGER_LITERAL
	| BIN_INTEGER_LITERAL
	| REAL_LITERAL
	| CHAR_LITERAL
	| NULL
;

boolean_literal
	: TRUE | FALSE
;

string_literal
	: interpolated_regular_string | interpolated_verbatium_string | REGULAR_STRING | VERBATIUM_STRING
;

interpolated_regular_string
	: INTERPOLATED_REGULAR_STRING_START interpolated_regular_string_part* DOUBLE_QUOTE_INSIDE
;

interpolated_verbatium_string
	: INTERPOLATED_REGULAR_STRING_START interpolated_verbatium_string_part* DOUBLE_QUOTE_INSIDE
;

interpolated_regular_string_part
	: interpolated_string_expression
	| DOUBLE_CURLY_INSIDE
	| REGULAR_CHAR_INSIDE
	| REGULAR_STRING_INSIDE
;

interpolated_verbatium_string_part
	: interpolated_string_expression
	| DOUBLE_CURLY_INSIDE
	| VERBATIUM_DOUBLE_QUOTE_INSIDE
	| VERBATIUM_INSIDE_STRING
;

interpolated_string_expression
	: expression (COMMA expression)* (COLON FORMAT_STRING+)?
;

identifier
	: IDENTIFIER | ALIAS | EXTERN | GLOBAL | GLOBALS | STATIC | VAR
;