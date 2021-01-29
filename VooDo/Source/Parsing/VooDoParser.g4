parser grammar VooDoParser
;

options
{
	tokenVocab = VooDoLexer;
}

// entry point
script
	: extern_alias_directives? using_directives? global_declarations? body EOF
;

global_declarations
	: GLOBALS OPEN_BRACE declarationStatement* CLOSE_BRACE
;

body
	: statement*
;

//B.2 Syntactic grammar

//B.2.1 Basic concepts

namespace_or_type_name
	: ( identifier type_argument_list? | qualified_alias_member) ( '.' identifier type_argument_list?)*
;

//B.2.2 Types
type_
	: base_type ( '?' | rank_specifier | '*')*
;

base_type
	: simple_type
	| class_type // represents types: enum, class, interface, delegate, type_parameter
	| VOID '*'
	| tuple_type
;

tuple_type
	: '(' tuple_element ( ',' tuple_element)+ ')'
;

tuple_element
	: type_ identifier?
;

simple_type
	: numeric_type | BOOL
;

numeric_type
	: integral_type | floating_point_type | DECIMAL
;

integral_type
	: SBYTE | BYTE | SHORT | USHORT | INT | UINT | LONG | ULONG | CHAR
;

floating_point_type
	: FLOAT | DOUBLE
;

/** namespace_or_type_name, OBJECT, STRING */
class_type
	: namespace_or_type_name | OBJECT | DYNAMIC | STRING
;

type_argument_list
	: '<' type_ ( ',' type_)* '>'
;

//B.2.4 Expressions
argument_list
	: argument ( ',' argument)*
;

argument
	: ( identifier ':')? refout = ( REF | OUT | IN)? ( VAR | type_)? expression
;

expression
	: assignment | non_assignment_expression | REF non_assignment_expression
;

non_assignment_expression
	: conditional_expression
;

assignment
	: unary_expression assignment_operator expression | unary_expression '??=' throwable_expression
;

assignment_operator
	: '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '<<=' | right_shift_assignment
;

conditional_expression
	: null_coalescing_expression ( '?' throwable_expression ':' throwable_expression)?
;

null_coalescing_expression
	: conditional_or_expression ( '??' ( null_coalescing_expression | throw_expression))?
;

conditional_or_expression
	: conditional_and_expression ( OP_OR conditional_and_expression)*
;

conditional_and_expression
	: inclusive_or_expression ( OP_AND inclusive_or_expression)*
;

inclusive_or_expression
	: exclusive_or_expression ( '|' exclusive_or_expression)*
;

exclusive_or_expression
	: and_expression ( '^' and_expression)*
;

and_expression
	: equality_expression ( '&' equality_expression)*
;

equality_expression
	: relational_expression ( ( OP_EQ | OP_NE) relational_expression)*
;

relational_expression
	: shift_expression ( ( '<' | '>' | '<=' | '>=') shift_expression | IS isType | AS type_)*
;

shift_expression
	: additive_expression ( ( '<<' | right_shift) additive_expression)*
;

additive_expression
	: multiplicative_expression ( ( '+' | '-') multiplicative_expression)*
;

multiplicative_expression
	: switch_expression ( ( '*' | '/' | '%') switch_expression)*
;

switch_expression
	: range_expression ( 'switch' '{' ( switch_expression_arms ','?)? '}')?
;

switch_expression_arms
	: switch_expression_arm ( ',' switch_expression_arm)*
;

switch_expression_arm
	: expression case_guard? right_arrow throwable_expression
;

range_expression
	: unary_expression | unary_expression? OP_RANGE unary_expression?
;

// https://msdn.microsoft.com/library/6a71f45d(v=vs.110).aspx
unary_expression
	: primary_expression
	| '+' unary_expression
	| '-' unary_expression
	| BANG unary_expression
	| '~' unary_expression
	| '++' unary_expression
	| '--' unary_expression
	| OPEN_PARENS type_ CLOSE_PARENS unary_expression
	| '&' unary_expression
	| '*' unary_expression
	| '^' unary_expression // C# 8 ranges
;

primary_expression
	: // Null-conditional operators C# 6: https://msdn.microsoft.com/en-us/library/dn986595.aspx
	pe = primary_expression_start '!'? bracket_expression* '!'?
	(
		( ( member_access | method_invocation | '++' | '--') '!'?) bracket_expression* '!'?
	)*
;

primary_expression_start
	: literal																# literalExpression
	| identifier type_argument_list?										# simpleNameExpression
	| OPEN_PARENS expression CLOSE_PARENS									# parenthesisExpressions
	| predefined_type														# memberAccessExpression
	| qualified_alias_member												# memberAccessExpression
	| LITERAL_ACCESS														# literalAccessExpression
	| THIS																	# thisReferenceExpression
	| BASE ('.' identifier type_argument_list? | '[' expression_list ']')	# baseAccessExpression
	| NEW
	(
		type_
		(
			object_creation_expression
			| object_or_collection_initializer
			| '[' expression_list ']' rank_specifier* array_initializer?
			| rank_specifier+ array_initializer
		)
		| anonymous_object_initializer
		| rank_specifier array_initializer
	)																		# objectCreationExpression
	| OPEN_PARENS argument ( ',' argument)+ CLOSE_PARENS					# tupleExpression
	| TYPEOF OPEN_PARENS (unbound_type_name | type_ | VOID) CLOSE_PARENS	# typeofExpression
	| CHECKED OPEN_PARENS expression CLOSE_PARENS							# checkedExpression
	| UNCHECKED OPEN_PARENS expression CLOSE_PARENS							# uncheckedExpression
	| DEFAULT (OPEN_PARENS type_ CLOSE_PARENS)?								# defaultValueExpression
	// C# 6: https://msdn.microsoft.com/en-us/library/dn986596.aspx
	| NAMEOF OPEN_PARENS (identifier '.')* identifier CLOSE_PARENS # nameofExpression
;

throwable_expression
	: expression | throw_expression
;

throw_expression
	: THROW expression
;

member_access
	: '?'? '.' identifier type_argument_list?
;

bracket_expression
	: '?'? '[' indexer_argument ( ',' indexer_argument)* ']'
;

indexer_argument
	: ( identifier ':')? expression
;

predefined_type
	: BOOL
	| BYTE
	| CHAR
	| DECIMAL
	| DOUBLE
	| FLOAT
	| INT
	| LONG
	| OBJECT
	| SBYTE
	| SHORT
	| STRING
	| UINT
	| ULONG
	| USHORT
;

expression_list
	: expression ( ',' expression)*
;

object_or_collection_initializer
	: object_initializer | collection_initializer
;

object_initializer
	: OPEN_BRACE ( member_initializer_list ','?)? CLOSE_BRACE
;

member_initializer_list
	: member_initializer ( ',' member_initializer)*
;

member_initializer
	: (identifier | '[' expression ']') '=' initializer_value // C# 6
;

initializer_value
	: expression | object_or_collection_initializer
;

collection_initializer
	: OPEN_BRACE element_initializer ( ',' element_initializer)* ','? CLOSE_BRACE
;

element_initializer
	: non_assignment_expression | OPEN_BRACE expression_list CLOSE_BRACE
;

anonymous_object_initializer
	: OPEN_BRACE ( member_declarator_list ','?)? CLOSE_BRACE
;

member_declarator_list
	: member_declarator ( ',' member_declarator)*
;

member_declarator
	: primary_expression | identifier '=' expression
;

unbound_type_name
	: identifier (generic_dimension_specifier? | '::' identifier generic_dimension_specifier?)
	(
		'.' identifier generic_dimension_specifier?
	)*
;

generic_dimension_specifier
	: '<' ','* '>'
;

isType
	: base_type ( rank_specifier | '*')* '?'? isTypePatternArms? identifier?
;

isTypePatternArms
	: '{' isTypePatternArm ( ',' isTypePatternArm)* '}'
;

isTypePatternArm
	: identifier ':' expression
;

//B.2.5 Statements
statement
	: declarationStatement | embedded_statement
;

declarationStatement
	: local_variable_declaration ';' | local_constant_declaration ';'
;

embedded_statement
	: block | simple_embedded_statement
;

simple_embedded_statement
	: ';'				# theEmptyStatement
	| expression ';'	# expressionStatement

	// selection statements
	| IF OPEN_PARENS expression CLOSE_PARENS if_body (ELSE if_body)?					# ifStatement
	| SWITCH OPEN_PARENS expression CLOSE_PARENS OPEN_BRACE switch_section* CLOSE_BRACE	# switchStatement

	// jump statements
	| THROW expression? ';'														# throwStatement
	| TRY block (catch_clauses finally_clause? | finally_clause)				# tryStatement
	| CHECKED block																# checkedStatement
	| UNCHECKED block															# uncheckedStatement
	| LOCK OPEN_PARENS expression CLOSE_PARENS embedded_statement				# lockStatement
	| USING OPEN_PARENS resource_acquisition CLOSE_PARENS embedded_statement	# usingStatement
;

block
	: OPEN_BRACE statement_list? CLOSE_BRACE
;

local_variable_declaration
	: (USING | REF | REF READONLY)? local_variable_type local_variable_declarator (',' local_variable_declarator)*
;

local_variable_type
	: VAR | type_
;

local_variable_declarator
	: identifier ( '=' REF? local_variable_initializer)?
;

local_variable_initializer
	: expression | array_initializer
;

local_constant_declaration
	: CONST type_ constant_declarators
;

if_body
	: block | simple_embedded_statement
;

switch_section
	: switch_label+ statement_list
;

switch_label
	: CASE expression case_guard? ':' | DEFAULT ':'
;

case_guard
	: WHEN expression
;

statement_list
	: statement+
;

catch_clauses
	: specific_catch_clause ( specific_catch_clause)* general_catch_clause? | general_catch_clause
;

specific_catch_clause
	: CATCH OPEN_PARENS class_type identifier? CLOSE_PARENS exception_filter? block
;

general_catch_clause
	: CATCH exception_filter? block
;

exception_filter
	: WHEN OPEN_PARENS expression CLOSE_PARENS
; // C# 6

finally_clause
	: FINALLY block
;

resource_acquisition
	: local_variable_declaration | expression
;

//B.2.6 Namespaces;

qualified_identifier
	: identifier ( '.' identifier)*
;

extern_alias_directives
	: extern_alias_directive+
;

extern_alias_directive
	: EXTERN ALIAS identifier ';'
;

using_directives
	: using_directive+
;

using_directive
	: USING identifier '=' namespace_or_type_name ';'	# usingAliasDirective
	| USING namespace_or_type_name ';'					# usingNamespaceDirective
	// C# 6: https://msdn.microsoft.com/en-us/library/ms228593.aspx
	| USING STATIC namespace_or_type_name ';' # usingStaticDirective
;

qualified_alias_member
	: identifier '::' identifier type_argument_list?
;

//B.2.7 Classes;
type_parameter_list
	: '<' type_parameter ( ',' type_parameter)* '>'
;

type_parameter
	: identifier
;

constant_declarators
	: constant_declarator ( ',' constant_declarator)*
;

constant_declarator
	: identifier '=' expression
;

variable_declarators
	: variable_declarator ( ',' variable_declarator)*
;

variable_declarator
	: identifier ( '=' variable_initializer)?
;

variable_initializer
	: expression | array_initializer
;

//B.2.9 Arrays
array_type
	: base_type ( ( '*' | '?')* rank_specifier)+
;

rank_specifier
	: '[' ','* ']'
;

array_initializer
	: OPEN_BRACE ( variable_initializer ( ',' variable_initializer)* ','?)? CLOSE_BRACE
;

right_arrow
	: first = '=' second = '>' {$first.index + 1 == $second.index}? // Nothing between the tokens?
;

right_shift
	: first = '>' second = '>' {$first.index + 1 == $second.index}? // Nothing between the tokens?
;

right_shift_assignment
	: first = '>' second = '>=' {$first.index + 1 == $second.index}? // Nothing between the tokens?
;

literal
	: boolean_literal
	| string_literal
	| INTEGER_LITERAL
	| HEX_INTEGER_LITERAL
	| BIN_INTEGER_LITERAL
	| REAL_LITERAL
	| CHARACTER_LITERAL
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
	: INTERPOLATED_VERBATIUM_STRING_START interpolated_verbatium_string_part* DOUBLE_QUOTE_INSIDE
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
	: expression ( ',' expression)* ( ':' FORMAT_STRING+)?
;

//B.1.7 Keywords
keyword
	: ABSTRACT
	| AS
	| BASE
	| BOOL
	| BREAK
	| BYTE
	| CASE
	| CATCH
	| CHAR
	| CHECKED
	| CLASS
	| CONST
	| CONTINUE
	| DECIMAL
	| DEFAULT
	| DELEGATE
	| DO
	| DOUBLE
	| ELSE
	| ENUM
	| EVENT
	| EXPLICIT
	| EXTERN
	| FALSE
	| FINALLY
	| FIXED
	| FLOAT
	| FOR
	| FOREACH
	| GLOBALS
	| GOTO
	| IF
	| IMPLICIT
	| IN
	| INT
	| INTERFACE
	| INTERNAL
	| IS
	| LOCK
	| LONG
	| NAMESPACE
	| NEW
	| NULL
	| OBJECT
	| OPERATOR
	| OUT
	| OVERRIDE
	| PARAMS
	| PRIVATE
	| PROTECTED
	| PUBLIC
	| READONLY
	| REF
	| RETURN
	| SBYTE
	| SEALED
	| SHORT
	| SIZEOF
	| STACKALLOC
	| STATIC
	| STRING
	| STRUCT
	| SWITCH
	| THIS
	| THROW
	| TRUE
	| TRY
	| TYPEOF
	| UINT
	| ULONG
	| UNCHECKED
	| UNMANAGED
	| UNSAFE
	| USHORT
	| USING
	| VIRTUAL
	| VOID
	| VOLATILE
	| WHILE
;

// -------------------- extra rules for modularization --------------------------------

method_invocation
	: OPEN_PARENS argument_list? CLOSE_PARENS
;

object_creation_expression
	: OPEN_PARENS argument_list? CLOSE_PARENS object_or_collection_initializer?
;

identifier
	: IDENTIFIER
	| ADD
	| ALIAS
	| ARGLIST
	| ASCENDING
	| ASYNC
	| AWAIT
	| BY
	| DESCENDING
	| DYNAMIC
	| EQUALS
	| FROM
	| GET
	| GLOBALS
	| GROUP
	| INTO
	| JOIN
	| LET
	| NAMEOF
	| ON
	| ORDERBY
	| PARTIAL
	| REMOVE
	| SELECT
	| SET
	| UNMANAGED
	| VAR
	| WHEN
	| WHERE
	| YIELD
;