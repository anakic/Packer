// Generated from c:\Projects\Packer\PBI_Expr_Antlr\pbiqParser.g4 by ANTLR 4.9.2
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class pbiqParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.9.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		FROM=1, AS=2, IN=3, WITH=4, NATIVEREFERENCENAME=5, DOT=6, AND=7, OR=8, 
		NOT=9, WHERE=10, ORDERBY=11, ASCENDING=12, DESCENDING=13, SELECT=14, SKIP_=15, 
		TOP=16, ANYVALUE=17, DEFAULTVALUEOVERRIDESANCESTORS=18, TRANSFORM=19, 
		VIA=20, NULL=21, TRUE=22, FALSE=23, BETWEEN=24, CONTAINS=25, AS_=26, INTEGER=27, 
		DECIMAL=28, DATE=29, DATETIMESECOND=30, DATETIME=31, SINGLE_QUOTE=32, 
		LPAREN=33, COMMA=34, RPAREN=35, GT=36, GTE=37, LT=38, LTE=39, EQ=40, STRING_LITERAL=41, 
		IDENTIFIER=42, BINARY_ARITHMETIC_OPERATOR=43, WS=44;
	public static final int
		RULE_query = 0, RULE_filter = 1, RULE_from = 2, RULE_where = 3, RULE_alias = 4, 
		RULE_entity = 5, RULE_entity_name = 6, RULE_schema = 7, RULE_expressionContainer = 8, 
		RULE_orderby = 9, RULE_groupby = 10, RULE_skip = 11, RULE_top = 12, RULE_direction = 13, 
		RULE_algorithm = 14, RULE_select = 15, RULE_expression = 16, RULE_nonFilterExpression = 17, 
		RULE_filterExpression = 18, RULE_queryFilter = 19, RULE_aggregationExpr = 20, 
		RULE_anyValueExpr = 21, RULE_andExpr = 22, RULE_betweenExpr = 23, RULE_nullEpr = 24, 
		RULE_intExpr = 25, RULE_decimalExpr = 26, RULE_datetimeExpr = 27, RULE_dateExpr = 28, 
		RULE_datetimeSecExpr = 29, RULE_containsExpr = 30, RULE_stringExpr = 31, 
		RULE_boolExp = 32, RULE_orExpr = 33, RULE_comparisonExpr = 34, RULE_propertyExpression = 35, 
		RULE_notExpr = 36, RULE_literalExpr = 37, RULE_inExpr = 38, RULE_arithmenticExpr = 39, 
		RULE_orderByClause = 40, RULE_left = 41, RULE_right = 42, RULE_ubound = 43, 
		RULE_lbound = 44, RULE_operator = 45;
	private static String[] makeRuleNames() {
		return new String[] {
			"query", "filter", "from", "where", "alias", "entity", "entity_name", 
			"schema", "expressionContainer", "orderby", "groupby", "skip", "top", 
			"direction", "algorithm", "select", "expression", "nonFilterExpression", 
			"filterExpression", "queryFilter", "aggregationExpr", "anyValueExpr", 
			"andExpr", "betweenExpr", "nullEpr", "intExpr", "decimalExpr", "datetimeExpr", 
			"dateExpr", "datetimeSecExpr", "containsExpr", "stringExpr", "boolExp", 
			"orExpr", "comparisonExpr", "propertyExpression", "notExpr", "literalExpr", 
			"inExpr", "arithmenticExpr", "orderByClause", "left", "right", "ubound", 
			"lbound", "operator"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null, 
			null, null, null, null, null, null, null, null, null, null, null, null, 
			null, null, null, null, null, null, null, null, "'''", "'('", "','", 
			"')'", "'>'", "'>='", "'<'", "'<='", "'='"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "FROM", "AS", "IN", "WITH", "NATIVEREFERENCENAME", "DOT", "AND", 
			"OR", "NOT", "WHERE", "ORDERBY", "ASCENDING", "DESCENDING", "SELECT", 
			"SKIP_", "TOP", "ANYVALUE", "DEFAULTVALUEOVERRIDESANCESTORS", "TRANSFORM", 
			"VIA", "NULL", "TRUE", "FALSE", "BETWEEN", "CONTAINS", "AS_", "INTEGER", 
			"DECIMAL", "DATE", "DATETIMESECOND", "DATETIME", "SINGLE_QUOTE", "LPAREN", 
			"COMMA", "RPAREN", "GT", "GTE", "LT", "LTE", "EQ", "STRING_LITERAL", 
			"IDENTIFIER", "BINARY_ARITHMETIC_OPERATOR", "WS"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}

	@Override
	public String getGrammarFileName() { return "pbiqParser.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public pbiqParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	public static class QueryContext extends ParserRuleContext {
		public FromContext from() {
			return getRuleContext(FromContext.class,0);
		}
		public TerminalNode EOF() { return getToken(pbiqParser.EOF, 0); }
		public WhereContext where() {
			return getRuleContext(WhereContext.class,0);
		}
		public OrderbyContext orderby() {
			return getRuleContext(OrderbyContext.class,0);
		}
		public SelectContext select() {
			return getRuleContext(SelectContext.class,0);
		}
		public GroupbyContext groupby() {
			return getRuleContext(GroupbyContext.class,0);
		}
		public SkipContext skip() {
			return getRuleContext(SkipContext.class,0);
		}
		public TopContext top() {
			return getRuleContext(TopContext.class,0);
		}
		public QueryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_query; }
	}

	public final QueryContext query() throws RecognitionException {
		QueryContext _localctx = new QueryContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_query);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(92);
			from();
			setState(94);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==WHERE) {
				{
				setState(93);
				where();
				}
			}

			setState(97);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,1,_ctx) ) {
			case 1:
				{
				setState(96);
				orderby();
				}
				break;
			}
			setState(100);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==SELECT) {
				{
				setState(99);
				select();
				}
			}

			setState(103);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ORDERBY) {
				{
				setState(102);
				groupby();
				}
			}

			setState(106);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==SKIP_) {
				{
				setState(105);
				skip();
				}
			}

			setState(109);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==TOP) {
				{
				setState(108);
				top();
				}
			}

			setState(111);
			match(EOF);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class FilterContext extends ParserRuleContext {
		public FromContext from() {
			return getRuleContext(FromContext.class,0);
		}
		public WhereContext where() {
			return getRuleContext(WhereContext.class,0);
		}
		public TerminalNode EOF() { return getToken(pbiqParser.EOF, 0); }
		public FilterContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_filter; }
	}

	public final FilterContext filter() throws RecognitionException {
		FilterContext _localctx = new FilterContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_filter);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(113);
			from();
			setState(114);
			where();
			setState(115);
			match(EOF);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class FromContext extends ParserRuleContext {
		public TerminalNode FROM() { return getToken(pbiqParser.FROM, 0); }
		public AliasContext alias() {
			return getRuleContext(AliasContext.class,0);
		}
		public TerminalNode IN() { return getToken(pbiqParser.IN, 0); }
		public EntityContext entity() {
			return getRuleContext(EntityContext.class,0);
		}
		public ExpressionContainerContext expressionContainer() {
			return getRuleContext(ExpressionContainerContext.class,0);
		}
		public FromContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_from; }
	}

	public final FromContext from() throws RecognitionException {
		FromContext _localctx = new FromContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_from);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(117);
			match(FROM);
			setState(118);
			alias();
			setState(119);
			match(IN);
			setState(122);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,6,_ctx) ) {
			case 1:
				{
				setState(120);
				entity();
				}
				break;
			case 2:
				{
				setState(121);
				expressionContainer();
				}
				break;
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class WhereContext extends ParserRuleContext {
		public TerminalNode WHERE() { return getToken(pbiqParser.WHERE, 0); }
		public QueryFilterContext queryFilter() {
			return getRuleContext(QueryFilterContext.class,0);
		}
		public WhereContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_where; }
	}

	public final WhereContext where() throws RecognitionException {
		WhereContext _localctx = new WhereContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_where);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(124);
			match(WHERE);
			setState(125);
			queryFilter();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class AliasContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(pbiqParser.IDENTIFIER, 0); }
		public AliasContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_alias; }
	}

	public final AliasContext alias() throws RecognitionException {
		AliasContext _localctx = new AliasContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_alias);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(127);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class EntityContext extends ParserRuleContext {
		public Entity_nameContext entity_name() {
			return getRuleContext(Entity_nameContext.class,0);
		}
		public SchemaContext schema() {
			return getRuleContext(SchemaContext.class,0);
		}
		public TerminalNode DOT() { return getToken(pbiqParser.DOT, 0); }
		public EntityContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_entity; }
	}

	public final EntityContext entity() throws RecognitionException {
		EntityContext _localctx = new EntityContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_entity);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(132);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,7,_ctx) ) {
			case 1:
				{
				setState(129);
				schema();
				setState(130);
				match(DOT);
				}
				break;
			}
			setState(134);
			entity_name();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class Entity_nameContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(pbiqParser.IDENTIFIER, 0); }
		public Entity_nameContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_entity_name; }
	}

	public final Entity_nameContext entity_name() throws RecognitionException {
		Entity_nameContext _localctx = new Entity_nameContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_entity_name);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(136);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class SchemaContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(pbiqParser.IDENTIFIER, 0); }
		public SchemaContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_schema; }
	}

	public final SchemaContext schema() throws RecognitionException {
		SchemaContext _localctx = new SchemaContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_schema);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(138);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ExpressionContainerContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode AS() { return getToken(pbiqParser.AS, 0); }
		public AliasContext alias() {
			return getRuleContext(AliasContext.class,0);
		}
		public TerminalNode WITH() { return getToken(pbiqParser.WITH, 0); }
		public TerminalNode NATIVEREFERENCENAME() { return getToken(pbiqParser.NATIVEREFERENCENAME, 0); }
		public ExpressionContainerContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_expressionContainer; }
	}

	public final ExpressionContainerContext expressionContainer() throws RecognitionException {
		ExpressionContainerContext _localctx = new ExpressionContainerContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_expressionContainer);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(140);
			expression();
			{
			setState(141);
			match(AS);
			setState(142);
			alias();
			}
			{
			setState(144);
			match(WITH);
			setState(145);
			match(NATIVEREFERENCENAME);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class OrderbyContext extends ParserRuleContext {
		public TerminalNode ORDERBY() { return getToken(pbiqParser.ORDERBY, 0); }
		public List<ExpressionContext> expression() {
			return getRuleContexts(ExpressionContext.class);
		}
		public ExpressionContext expression(int i) {
			return getRuleContext(ExpressionContext.class,i);
		}
		public List<DirectionContext> direction() {
			return getRuleContexts(DirectionContext.class);
		}
		public DirectionContext direction(int i) {
			return getRuleContext(DirectionContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(pbiqParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(pbiqParser.COMMA, i);
		}
		public OrderbyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_orderby; }
	}

	public final OrderbyContext orderby() throws RecognitionException {
		OrderbyContext _localctx = new OrderbyContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_orderby);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(147);
			match(ORDERBY);
			setState(148);
			expression();
			setState(149);
			direction();
			setState(156);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(150);
				match(COMMA);
				setState(151);
				expression();
				setState(152);
				direction();
				}
				}
				setState(158);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class GroupbyContext extends ParserRuleContext {
		public TerminalNode ORDERBY() { return getToken(pbiqParser.ORDERBY, 0); }
		public List<ExpressionContext> expression() {
			return getRuleContexts(ExpressionContext.class);
		}
		public ExpressionContext expression(int i) {
			return getRuleContext(ExpressionContext.class,i);
		}
		public List<DirectionContext> direction() {
			return getRuleContexts(DirectionContext.class);
		}
		public DirectionContext direction(int i) {
			return getRuleContext(DirectionContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(pbiqParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(pbiqParser.COMMA, i);
		}
		public GroupbyContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_groupby; }
	}

	public final GroupbyContext groupby() throws RecognitionException {
		GroupbyContext _localctx = new GroupbyContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_groupby);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(159);
			match(ORDERBY);
			setState(160);
			expression();
			setState(161);
			direction();
			setState(168);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(162);
				match(COMMA);
				setState(163);
				expression();
				setState(164);
				direction();
				}
				}
				setState(170);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class SkipContext extends ParserRuleContext {
		public TerminalNode SKIP_() { return getToken(pbiqParser.SKIP_, 0); }
		public TerminalNode INTEGER() { return getToken(pbiqParser.INTEGER, 0); }
		public SkipContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_skip; }
	}

	public final SkipContext skip() throws RecognitionException {
		SkipContext _localctx = new SkipContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_skip);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(171);
			match(SKIP_);
			setState(172);
			match(INTEGER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class TopContext extends ParserRuleContext {
		public TerminalNode TOP() { return getToken(pbiqParser.TOP, 0); }
		public TerminalNode INTEGER() { return getToken(pbiqParser.INTEGER, 0); }
		public TopContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_top; }
	}

	public final TopContext top() throws RecognitionException {
		TopContext _localctx = new TopContext(_ctx, getState());
		enterRule(_localctx, 24, RULE_top);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(174);
			match(TOP);
			setState(175);
			match(INTEGER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class DirectionContext extends ParserRuleContext {
		public TerminalNode ASCENDING() { return getToken(pbiqParser.ASCENDING, 0); }
		public TerminalNode DESCENDING() { return getToken(pbiqParser.DESCENDING, 0); }
		public DirectionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_direction; }
	}

	public final DirectionContext direction() throws RecognitionException {
		DirectionContext _localctx = new DirectionContext(_ctx, getState());
		enterRule(_localctx, 26, RULE_direction);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(177);
			_la = _input.LA(1);
			if ( !(_la==ASCENDING || _la==DESCENDING) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class AlgorithmContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(pbiqParser.IDENTIFIER, 0); }
		public AlgorithmContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_algorithm; }
	}

	public final AlgorithmContext algorithm() throws RecognitionException {
		AlgorithmContext _localctx = new AlgorithmContext(_ctx, getState());
		enterRule(_localctx, 28, RULE_algorithm);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(179);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class SelectContext extends ParserRuleContext {
		public TerminalNode SELECT() { return getToken(pbiqParser.SELECT, 0); }
		public List<ExpressionContext> expression() {
			return getRuleContexts(ExpressionContext.class);
		}
		public ExpressionContext expression(int i) {
			return getRuleContext(ExpressionContext.class,i);
		}
		public List<TerminalNode> COMMA() { return getTokens(pbiqParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(pbiqParser.COMMA, i);
		}
		public SelectContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_select; }
	}

	public final SelectContext select() throws RecognitionException {
		SelectContext _localctx = new SelectContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_select);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(181);
			match(SELECT);
			setState(182);
			expression();
			setState(187);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(183);
				match(COMMA);
				setState(184);
				expression();
				}
				}
				setState(189);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ExpressionContext extends ParserRuleContext {
		public FilterExpressionContext filterExpression() {
			return getRuleContext(FilterExpressionContext.class,0);
		}
		public NonFilterExpressionContext nonFilterExpression() {
			return getRuleContext(NonFilterExpressionContext.class,0);
		}
		public ExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_expression; }
	}

	public final ExpressionContext expression() throws RecognitionException {
		ExpressionContext _localctx = new ExpressionContext(_ctx, getState());
		enterRule(_localctx, 32, RULE_expression);
		try {
			setState(192);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,11,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(190);
				filterExpression();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(191);
				nonFilterExpression();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class NonFilterExpressionContext extends ParserRuleContext {
		public AggregationExprContext aggregationExpr() {
			return getRuleContext(AggregationExprContext.class,0);
		}
		public ArithmenticExprContext arithmenticExpr() {
			return getRuleContext(ArithmenticExprContext.class,0);
		}
		public AnyValueExprContext anyValueExpr() {
			return getRuleContext(AnyValueExprContext.class,0);
		}
		public LiteralExprContext literalExpr() {
			return getRuleContext(LiteralExprContext.class,0);
		}
		public NullEprContext nullEpr() {
			return getRuleContext(NullEprContext.class,0);
		}
		public IntExprContext intExpr() {
			return getRuleContext(IntExprContext.class,0);
		}
		public DecimalExprContext decimalExpr() {
			return getRuleContext(DecimalExprContext.class,0);
		}
		public StringExprContext stringExpr() {
			return getRuleContext(StringExprContext.class,0);
		}
		public DatetimeExprContext datetimeExpr() {
			return getRuleContext(DatetimeExprContext.class,0);
		}
		public DatetimeSecExprContext datetimeSecExpr() {
			return getRuleContext(DatetimeSecExprContext.class,0);
		}
		public DateExprContext dateExpr() {
			return getRuleContext(DateExprContext.class,0);
		}
		public PropertyExpressionContext propertyExpression() {
			return getRuleContext(PropertyExpressionContext.class,0);
		}
		public NonFilterExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_nonFilterExpression; }
	}

	public final NonFilterExpressionContext nonFilterExpression() throws RecognitionException {
		NonFilterExpressionContext _localctx = new NonFilterExpressionContext(_ctx, getState());
		enterRule(_localctx, 34, RULE_nonFilterExpression);
		try {
			setState(206);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,12,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(194);
				aggregationExpr();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(195);
				arithmenticExpr();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(196);
				anyValueExpr();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(197);
				literalExpr();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(198);
				nullEpr();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(199);
				intExpr();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(200);
				decimalExpr();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(201);
				stringExpr();
				}
				break;
			case 9:
				enterOuterAlt(_localctx, 9);
				{
				setState(202);
				datetimeExpr();
				}
				break;
			case 10:
				enterOuterAlt(_localctx, 10);
				{
				setState(203);
				datetimeSecExpr();
				}
				break;
			case 11:
				enterOuterAlt(_localctx, 11);
				{
				setState(204);
				dateExpr();
				}
				break;
			case 12:
				enterOuterAlt(_localctx, 12);
				{
				setState(205);
				propertyExpression();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class FilterExpressionContext extends ParserRuleContext {
		public AndExprContext andExpr() {
			return getRuleContext(AndExprContext.class,0);
		}
		public BetweenExprContext betweenExpr() {
			return getRuleContext(BetweenExprContext.class,0);
		}
		public NotExprContext notExpr() {
			return getRuleContext(NotExprContext.class,0);
		}
		public BoolExpContext boolExp() {
			return getRuleContext(BoolExpContext.class,0);
		}
		public InExprContext inExpr() {
			return getRuleContext(InExprContext.class,0);
		}
		public ComparisonExprContext comparisonExpr() {
			return getRuleContext(ComparisonExprContext.class,0);
		}
		public ContainsExprContext containsExpr() {
			return getRuleContext(ContainsExprContext.class,0);
		}
		public FilterExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_filterExpression; }
	}

	public final FilterExpressionContext filterExpression() throws RecognitionException {
		FilterExpressionContext _localctx = new FilterExpressionContext(_ctx, getState());
		enterRule(_localctx, 36, RULE_filterExpression);
		try {
			setState(216);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,13,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(209);
				andExpr();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(210);
				betweenExpr();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(211);
				notExpr();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(212);
				boolExp();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(213);
				inExpr();
				}
				break;
			case 7:
				enterOuterAlt(_localctx, 7);
				{
				setState(214);
				comparisonExpr();
				}
				break;
			case 8:
				enterOuterAlt(_localctx, 8);
				{
				setState(215);
				containsExpr();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class QueryFilterContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public QueryFilterContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_queryFilter; }
	}

	public final QueryFilterContext queryFilter() throws RecognitionException {
		QueryFilterContext _localctx = new QueryFilterContext(_ctx, getState());
		enterRule(_localctx, 38, RULE_queryFilter);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(218);
			expression();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class AggregationExprContext extends ParserRuleContext {
		public TerminalNode IDENTIFIER() { return getToken(pbiqParser.IDENTIFIER, 0); }
		public TerminalNode LPAREN() { return getToken(pbiqParser.LPAREN, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode RPAREN() { return getToken(pbiqParser.RPAREN, 0); }
		public AggregationExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_aggregationExpr; }
	}

	public final AggregationExprContext aggregationExpr() throws RecognitionException {
		AggregationExprContext _localctx = new AggregationExprContext(_ctx, getState());
		enterRule(_localctx, 40, RULE_aggregationExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(220);
			match(IDENTIFIER);
			setState(221);
			match(LPAREN);
			setState(222);
			expression();
			setState(223);
			match(RPAREN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class AnyValueExprContext extends ParserRuleContext {
		public TerminalNode ANYVALUE() { return getToken(pbiqParser.ANYVALUE, 0); }
		public TerminalNode WITH() { return getToken(pbiqParser.WITH, 0); }
		public TerminalNode DEFAULTVALUEOVERRIDESANCESTORS() { return getToken(pbiqParser.DEFAULTVALUEOVERRIDESANCESTORS, 0); }
		public AnyValueExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_anyValueExpr; }
	}

	public final AnyValueExprContext anyValueExpr() throws RecognitionException {
		AnyValueExprContext _localctx = new AnyValueExprContext(_ctx, getState());
		enterRule(_localctx, 42, RULE_anyValueExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(225);
			match(ANYVALUE);
			setState(226);
			match(WITH);
			setState(227);
			match(DEFAULTVALUEOVERRIDESANCESTORS);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class AndExprContext extends ParserRuleContext {
		public TerminalNode LPAREN() { return getToken(pbiqParser.LPAREN, 0); }
		public LeftContext left() {
			return getRuleContext(LeftContext.class,0);
		}
		public TerminalNode AND() { return getToken(pbiqParser.AND, 0); }
		public RightContext right() {
			return getRuleContext(RightContext.class,0);
		}
		public TerminalNode RPAREN() { return getToken(pbiqParser.RPAREN, 0); }
		public AndExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_andExpr; }
	}

	public final AndExprContext andExpr() throws RecognitionException {
		AndExprContext _localctx = new AndExprContext(_ctx, getState());
		enterRule(_localctx, 44, RULE_andExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(229);
			match(LPAREN);
			setState(230);
			left();
			setState(231);
			match(AND);
			setState(232);
			right();
			setState(233);
			match(RPAREN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class BetweenExprContext extends ParserRuleContext {
		public NonFilterExpressionContext nonFilterExpression() {
			return getRuleContext(NonFilterExpressionContext.class,0);
		}
		public TerminalNode BETWEEN() { return getToken(pbiqParser.BETWEEN, 0); }
		public UboundContext ubound() {
			return getRuleContext(UboundContext.class,0);
		}
		public TerminalNode AND() { return getToken(pbiqParser.AND, 0); }
		public LboundContext lbound() {
			return getRuleContext(LboundContext.class,0);
		}
		public BetweenExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_betweenExpr; }
	}

	public final BetweenExprContext betweenExpr() throws RecognitionException {
		BetweenExprContext _localctx = new BetweenExprContext(_ctx, getState());
		enterRule(_localctx, 46, RULE_betweenExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(235);
			nonFilterExpression();
			setState(236);
			match(BETWEEN);
			setState(237);
			ubound();
			setState(238);
			match(AND);
			setState(239);
			lbound();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class NullEprContext extends ParserRuleContext {
		public TerminalNode NULL() { return getToken(pbiqParser.NULL, 0); }
		public NullEprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_nullEpr; }
	}

	public final NullEprContext nullEpr() throws RecognitionException {
		NullEprContext _localctx = new NullEprContext(_ctx, getState());
		enterRule(_localctx, 48, RULE_nullEpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(241);
			match(NULL);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class IntExprContext extends ParserRuleContext {
		public TerminalNode INTEGER() { return getToken(pbiqParser.INTEGER, 0); }
		public IntExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_intExpr; }
	}

	public final IntExprContext intExpr() throws RecognitionException {
		IntExprContext _localctx = new IntExprContext(_ctx, getState());
		enterRule(_localctx, 50, RULE_intExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(243);
			match(INTEGER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class DecimalExprContext extends ParserRuleContext {
		public TerminalNode DECIMAL() { return getToken(pbiqParser.DECIMAL, 0); }
		public DecimalExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_decimalExpr; }
	}

	public final DecimalExprContext decimalExpr() throws RecognitionException {
		DecimalExprContext _localctx = new DecimalExprContext(_ctx, getState());
		enterRule(_localctx, 52, RULE_decimalExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(245);
			match(DECIMAL);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class DatetimeExprContext extends ParserRuleContext {
		public TerminalNode DATETIME() { return getToken(pbiqParser.DATETIME, 0); }
		public DatetimeExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_datetimeExpr; }
	}

	public final DatetimeExprContext datetimeExpr() throws RecognitionException {
		DatetimeExprContext _localctx = new DatetimeExprContext(_ctx, getState());
		enterRule(_localctx, 54, RULE_datetimeExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(247);
			match(DATETIME);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class DateExprContext extends ParserRuleContext {
		public TerminalNode DATE() { return getToken(pbiqParser.DATE, 0); }
		public DateExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_dateExpr; }
	}

	public final DateExprContext dateExpr() throws RecognitionException {
		DateExprContext _localctx = new DateExprContext(_ctx, getState());
		enterRule(_localctx, 56, RULE_dateExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(249);
			match(DATE);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class DatetimeSecExprContext extends ParserRuleContext {
		public TerminalNode DATETIMESECOND() { return getToken(pbiqParser.DATETIMESECOND, 0); }
		public DatetimeSecExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_datetimeSecExpr; }
	}

	public final DatetimeSecExprContext datetimeSecExpr() throws RecognitionException {
		DatetimeSecExprContext _localctx = new DatetimeSecExprContext(_ctx, getState());
		enterRule(_localctx, 58, RULE_datetimeSecExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(251);
			match(DATETIMESECOND);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ContainsExprContext extends ParserRuleContext {
		public LeftContext left() {
			return getRuleContext(LeftContext.class,0);
		}
		public TerminalNode CONTAINS() { return getToken(pbiqParser.CONTAINS, 0); }
		public RightContext right() {
			return getRuleContext(RightContext.class,0);
		}
		public ContainsExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_containsExpr; }
	}

	public final ContainsExprContext containsExpr() throws RecognitionException {
		ContainsExprContext _localctx = new ContainsExprContext(_ctx, getState());
		enterRule(_localctx, 60, RULE_containsExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(253);
			left();
			setState(254);
			match(CONTAINS);
			setState(255);
			right();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class StringExprContext extends ParserRuleContext {
		public TerminalNode STRING_LITERAL() { return getToken(pbiqParser.STRING_LITERAL, 0); }
		public StringExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_stringExpr; }
	}

	public final StringExprContext stringExpr() throws RecognitionException {
		StringExprContext _localctx = new StringExprContext(_ctx, getState());
		enterRule(_localctx, 62, RULE_stringExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(257);
			match(STRING_LITERAL);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class BoolExpContext extends ParserRuleContext {
		public TerminalNode TRUE() { return getToken(pbiqParser.TRUE, 0); }
		public TerminalNode FALSE() { return getToken(pbiqParser.FALSE, 0); }
		public BoolExpContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_boolExp; }
	}

	public final BoolExpContext boolExp() throws RecognitionException {
		BoolExpContext _localctx = new BoolExpContext(_ctx, getState());
		enterRule(_localctx, 64, RULE_boolExp);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(259);
			_la = _input.LA(1);
			if ( !(_la==TRUE || _la==FALSE) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class OrExprContext extends ParserRuleContext {
		public TerminalNode LPAREN() { return getToken(pbiqParser.LPAREN, 0); }
		public LeftContext left() {
			return getRuleContext(LeftContext.class,0);
		}
		public TerminalNode OR() { return getToken(pbiqParser.OR, 0); }
		public RightContext right() {
			return getRuleContext(RightContext.class,0);
		}
		public TerminalNode RPAREN() { return getToken(pbiqParser.RPAREN, 0); }
		public OrExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_orExpr; }
	}

	public final OrExprContext orExpr() throws RecognitionException {
		OrExprContext _localctx = new OrExprContext(_ctx, getState());
		enterRule(_localctx, 66, RULE_orExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(261);
			match(LPAREN);
			setState(262);
			left();
			setState(263);
			match(OR);
			setState(264);
			right();
			setState(265);
			match(RPAREN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ComparisonExprContext extends ParserRuleContext {
		public List<NonFilterExpressionContext> nonFilterExpression() {
			return getRuleContexts(NonFilterExpressionContext.class);
		}
		public NonFilterExpressionContext nonFilterExpression(int i) {
			return getRuleContext(NonFilterExpressionContext.class,i);
		}
		public OperatorContext operator() {
			return getRuleContext(OperatorContext.class,0);
		}
		public ComparisonExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_comparisonExpr; }
	}

	public final ComparisonExprContext comparisonExpr() throws RecognitionException {
		ComparisonExprContext _localctx = new ComparisonExprContext(_ctx, getState());
		enterRule(_localctx, 68, RULE_comparisonExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(267);
			nonFilterExpression();
			setState(268);
			operator();
			setState(269);
			nonFilterExpression();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class PropertyExpressionContext extends ParserRuleContext {
		public List<TerminalNode> IDENTIFIER() { return getTokens(pbiqParser.IDENTIFIER); }
		public TerminalNode IDENTIFIER(int i) {
			return getToken(pbiqParser.IDENTIFIER, i);
		}
		public TerminalNode DOT() { return getToken(pbiqParser.DOT, 0); }
		public PropertyExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_propertyExpression; }
	}

	public final PropertyExpressionContext propertyExpression() throws RecognitionException {
		PropertyExpressionContext _localctx = new PropertyExpressionContext(_ctx, getState());
		enterRule(_localctx, 70, RULE_propertyExpression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(271);
			match(IDENTIFIER);
			setState(272);
			match(DOT);
			setState(273);
			match(IDENTIFIER);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class NotExprContext extends ParserRuleContext {
		public TerminalNode NOT() { return getToken(pbiqParser.NOT, 0); }
		public TerminalNode LPAREN() { return getToken(pbiqParser.LPAREN, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode RPAREN() { return getToken(pbiqParser.RPAREN, 0); }
		public NotExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_notExpr; }
	}

	public final NotExprContext notExpr() throws RecognitionException {
		NotExprContext _localctx = new NotExprContext(_ctx, getState());
		enterRule(_localctx, 72, RULE_notExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(275);
			match(NOT);
			setState(276);
			match(LPAREN);
			setState(277);
			expression();
			setState(278);
			match(RPAREN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class LiteralExprContext extends ParserRuleContext {
		public TerminalNode STRING_LITERAL() { return getToken(pbiqParser.STRING_LITERAL, 0); }
		public LiteralExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_literalExpr; }
	}

	public final LiteralExprContext literalExpr() throws RecognitionException {
		LiteralExprContext _localctx = new LiteralExprContext(_ctx, getState());
		enterRule(_localctx, 74, RULE_literalExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(280);
			match(STRING_LITERAL);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class InExprContext extends ParserRuleContext {
		public NonFilterExpressionContext nonFilterExpression() {
			return getRuleContext(NonFilterExpressionContext.class,0);
		}
		public TerminalNode IN() { return getToken(pbiqParser.IN, 0); }
		public TerminalNode LPAREN() { return getToken(pbiqParser.LPAREN, 0); }
		public List<ExpressionContext> expression() {
			return getRuleContexts(ExpressionContext.class);
		}
		public ExpressionContext expression(int i) {
			return getRuleContext(ExpressionContext.class,i);
		}
		public TerminalNode RPAREN() { return getToken(pbiqParser.RPAREN, 0); }
		public List<TerminalNode> COMMA() { return getTokens(pbiqParser.COMMA); }
		public TerminalNode COMMA(int i) {
			return getToken(pbiqParser.COMMA, i);
		}
		public InExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_inExpr; }
	}

	public final InExprContext inExpr() throws RecognitionException {
		InExprContext _localctx = new InExprContext(_ctx, getState());
		enterRule(_localctx, 76, RULE_inExpr);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(282);
			nonFilterExpression();
			setState(283);
			match(IN);
			setState(284);
			match(LPAREN);
			setState(285);
			expression();
			setState(290);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==COMMA) {
				{
				{
				setState(286);
				match(COMMA);
				setState(287);
				expression();
				}
				}
				setState(292);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(293);
			match(RPAREN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ArithmenticExprContext extends ParserRuleContext {
		public TerminalNode LPAREN() { return getToken(pbiqParser.LPAREN, 0); }
		public LeftContext left() {
			return getRuleContext(LeftContext.class,0);
		}
		public TerminalNode BINARY_ARITHMETIC_OPERATOR() { return getToken(pbiqParser.BINARY_ARITHMETIC_OPERATOR, 0); }
		public RightContext right() {
			return getRuleContext(RightContext.class,0);
		}
		public TerminalNode RPAREN() { return getToken(pbiqParser.RPAREN, 0); }
		public ArithmenticExprContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_arithmenticExpr; }
	}

	public final ArithmenticExprContext arithmenticExpr() throws RecognitionException {
		ArithmenticExprContext _localctx = new ArithmenticExprContext(_ctx, getState());
		enterRule(_localctx, 78, RULE_arithmenticExpr);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(295);
			match(LPAREN);
			setState(296);
			left();
			setState(297);
			match(BINARY_ARITHMETIC_OPERATOR);
			setState(298);
			right();
			setState(299);
			match(RPAREN);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class OrderByClauseContext extends ParserRuleContext {
		public TerminalNode ORDERBY() { return getToken(pbiqParser.ORDERBY, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public OrderByClauseContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_orderByClause; }
	}

	public final OrderByClauseContext orderByClause() throws RecognitionException {
		OrderByClauseContext _localctx = new OrderByClauseContext(_ctx, getState());
		enterRule(_localctx, 80, RULE_orderByClause);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(301);
			match(ORDERBY);
			setState(302);
			expression();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class LeftContext extends ParserRuleContext {
		public NonFilterExpressionContext nonFilterExpression() {
			return getRuleContext(NonFilterExpressionContext.class,0);
		}
		public LeftContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_left; }
	}

	public final LeftContext left() throws RecognitionException {
		LeftContext _localctx = new LeftContext(_ctx, getState());
		enterRule(_localctx, 82, RULE_left);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(304);
			nonFilterExpression();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class RightContext extends ParserRuleContext {
		public NonFilterExpressionContext nonFilterExpression() {
			return getRuleContext(NonFilterExpressionContext.class,0);
		}
		public RightContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_right; }
	}

	public final RightContext right() throws RecognitionException {
		RightContext _localctx = new RightContext(_ctx, getState());
		enterRule(_localctx, 84, RULE_right);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(306);
			nonFilterExpression();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class UboundContext extends ParserRuleContext {
		public NonFilterExpressionContext nonFilterExpression() {
			return getRuleContext(NonFilterExpressionContext.class,0);
		}
		public UboundContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_ubound; }
	}

	public final UboundContext ubound() throws RecognitionException {
		UboundContext _localctx = new UboundContext(_ctx, getState());
		enterRule(_localctx, 86, RULE_ubound);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(308);
			nonFilterExpression();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class LboundContext extends ParserRuleContext {
		public NonFilterExpressionContext nonFilterExpression() {
			return getRuleContext(NonFilterExpressionContext.class,0);
		}
		public LboundContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_lbound; }
	}

	public final LboundContext lbound() throws RecognitionException {
		LboundContext _localctx = new LboundContext(_ctx, getState());
		enterRule(_localctx, 88, RULE_lbound);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(310);
			nonFilterExpression();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class OperatorContext extends ParserRuleContext {
		public TerminalNode GT() { return getToken(pbiqParser.GT, 0); }
		public TerminalNode LT() { return getToken(pbiqParser.LT, 0); }
		public TerminalNode EQ() { return getToken(pbiqParser.EQ, 0); }
		public TerminalNode GTE() { return getToken(pbiqParser.GTE, 0); }
		public TerminalNode LTE() { return getToken(pbiqParser.LTE, 0); }
		public OperatorContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_operator; }
	}

	public final OperatorContext operator() throws RecognitionException {
		OperatorContext _localctx = new OperatorContext(_ctx, getState());
		enterRule(_localctx, 90, RULE_operator);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(312);
			_la = _input.LA(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << GT) | (1L << GTE) | (1L << LT) | (1L << LTE) | (1L << EQ))) != 0)) ) {
			_errHandler.recoverInline(this);
			}
			else {
				if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
				_errHandler.reportMatch(this);
				consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\3.\u013d\4\2\t\2\4"+
		"\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13\t"+
		"\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22\t\22"+
		"\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31\t\31"+
		"\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t \4!"+
		"\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t+\4"+
		",\t,\4-\t-\4.\t.\4/\t/\3\2\3\2\5\2a\n\2\3\2\5\2d\n\2\3\2\5\2g\n\2\3\2"+
		"\5\2j\n\2\3\2\5\2m\n\2\3\2\5\2p\n\2\3\2\3\2\3\3\3\3\3\3\3\3\3\4\3\4\3"+
		"\4\3\4\3\4\5\4}\n\4\3\5\3\5\3\5\3\6\3\6\3\7\3\7\3\7\5\7\u0087\n\7\3\7"+
		"\3\7\3\b\3\b\3\t\3\t\3\n\3\n\3\n\3\n\3\n\3\n\3\n\3\13\3\13\3\13\3\13\3"+
		"\13\3\13\3\13\7\13\u009d\n\13\f\13\16\13\u00a0\13\13\3\f\3\f\3\f\3\f\3"+
		"\f\3\f\3\f\7\f\u00a9\n\f\f\f\16\f\u00ac\13\f\3\r\3\r\3\r\3\16\3\16\3\16"+
		"\3\17\3\17\3\20\3\20\3\21\3\21\3\21\3\21\7\21\u00bc\n\21\f\21\16\21\u00bf"+
		"\13\21\3\22\3\22\5\22\u00c3\n\22\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3"+
		"\23\3\23\3\23\3\23\3\23\5\23\u00d1\n\23\3\24\3\24\3\24\3\24\3\24\3\24"+
		"\3\24\3\24\5\24\u00db\n\24\3\25\3\25\3\26\3\26\3\26\3\26\3\26\3\27\3\27"+
		"\3\27\3\27\3\30\3\30\3\30\3\30\3\30\3\30\3\31\3\31\3\31\3\31\3\31\3\31"+
		"\3\32\3\32\3\33\3\33\3\34\3\34\3\35\3\35\3\36\3\36\3\37\3\37\3 \3 \3 "+
		"\3 \3!\3!\3\"\3\"\3#\3#\3#\3#\3#\3#\3$\3$\3$\3$\3%\3%\3%\3%\3&\3&\3&\3"+
		"&\3&\3\'\3\'\3(\3(\3(\3(\3(\3(\7(\u0123\n(\f(\16(\u0126\13(\3(\3(\3)\3"+
		")\3)\3)\3)\3)\3*\3*\3*\3+\3+\3,\3,\3-\3-\3.\3.\3/\3/\3/\2\2\60\2\4\6\b"+
		"\n\f\16\20\22\24\26\30\32\34\36 \"$&(*,.\60\62\64\668:<>@BDFHJLNPRTVX"+
		"Z\\\2\5\3\2\16\17\3\2\30\31\3\2&*\2\u012d\2^\3\2\2\2\4s\3\2\2\2\6w\3\2"+
		"\2\2\b~\3\2\2\2\n\u0081\3\2\2\2\f\u0086\3\2\2\2\16\u008a\3\2\2\2\20\u008c"+
		"\3\2\2\2\22\u008e\3\2\2\2\24\u0095\3\2\2\2\26\u00a1\3\2\2\2\30\u00ad\3"+
		"\2\2\2\32\u00b0\3\2\2\2\34\u00b3\3\2\2\2\36\u00b5\3\2\2\2 \u00b7\3\2\2"+
		"\2\"\u00c2\3\2\2\2$\u00d0\3\2\2\2&\u00da\3\2\2\2(\u00dc\3\2\2\2*\u00de"+
		"\3\2\2\2,\u00e3\3\2\2\2.\u00e7\3\2\2\2\60\u00ed\3\2\2\2\62\u00f3\3\2\2"+
		"\2\64\u00f5\3\2\2\2\66\u00f7\3\2\2\28\u00f9\3\2\2\2:\u00fb\3\2\2\2<\u00fd"+
		"\3\2\2\2>\u00ff\3\2\2\2@\u0103\3\2\2\2B\u0105\3\2\2\2D\u0107\3\2\2\2F"+
		"\u010d\3\2\2\2H\u0111\3\2\2\2J\u0115\3\2\2\2L\u011a\3\2\2\2N\u011c\3\2"+
		"\2\2P\u0129\3\2\2\2R\u012f\3\2\2\2T\u0132\3\2\2\2V\u0134\3\2\2\2X\u0136"+
		"\3\2\2\2Z\u0138\3\2\2\2\\\u013a\3\2\2\2^`\5\6\4\2_a\5\b\5\2`_\3\2\2\2"+
		"`a\3\2\2\2ac\3\2\2\2bd\5\24\13\2cb\3\2\2\2cd\3\2\2\2df\3\2\2\2eg\5 \21"+
		"\2fe\3\2\2\2fg\3\2\2\2gi\3\2\2\2hj\5\26\f\2ih\3\2\2\2ij\3\2\2\2jl\3\2"+
		"\2\2km\5\30\r\2lk\3\2\2\2lm\3\2\2\2mo\3\2\2\2np\5\32\16\2on\3\2\2\2op"+
		"\3\2\2\2pq\3\2\2\2qr\7\2\2\3r\3\3\2\2\2st\5\6\4\2tu\5\b\5\2uv\7\2\2\3"+
		"v\5\3\2\2\2wx\7\3\2\2xy\5\n\6\2y|\7\5\2\2z}\5\f\7\2{}\5\22\n\2|z\3\2\2"+
		"\2|{\3\2\2\2}\7\3\2\2\2~\177\7\f\2\2\177\u0080\5(\25\2\u0080\t\3\2\2\2"+
		"\u0081\u0082\7,\2\2\u0082\13\3\2\2\2\u0083\u0084\5\20\t\2\u0084\u0085"+
		"\7\b\2\2\u0085\u0087\3\2\2\2\u0086\u0083\3\2\2\2\u0086\u0087\3\2\2\2\u0087"+
		"\u0088\3\2\2\2\u0088\u0089\5\16\b\2\u0089\r\3\2\2\2\u008a\u008b\7,\2\2"+
		"\u008b\17\3\2\2\2\u008c\u008d\7,\2\2\u008d\21\3\2\2\2\u008e\u008f\5\""+
		"\22\2\u008f\u0090\7\4\2\2\u0090\u0091\5\n\6\2\u0091\u0092\3\2\2\2\u0092"+
		"\u0093\7\6\2\2\u0093\u0094\7\7\2\2\u0094\23\3\2\2\2\u0095\u0096\7\r\2"+
		"\2\u0096\u0097\5\"\22\2\u0097\u009e\5\34\17\2\u0098\u0099\7$\2\2\u0099"+
		"\u009a\5\"\22\2\u009a\u009b\5\34\17\2\u009b\u009d\3\2\2\2\u009c\u0098"+
		"\3\2\2\2\u009d\u00a0\3\2\2\2\u009e\u009c\3\2\2\2\u009e\u009f\3\2\2\2\u009f"+
		"\25\3\2\2\2\u00a0\u009e\3\2\2\2\u00a1\u00a2\7\r\2\2\u00a2\u00a3\5\"\22"+
		"\2\u00a3\u00aa\5\34\17\2\u00a4\u00a5\7$\2\2\u00a5\u00a6\5\"\22\2\u00a6"+
		"\u00a7\5\34\17\2\u00a7\u00a9\3\2\2\2\u00a8\u00a4\3\2\2\2\u00a9\u00ac\3"+
		"\2\2\2\u00aa\u00a8\3\2\2\2\u00aa\u00ab\3\2\2\2\u00ab\27\3\2\2\2\u00ac"+
		"\u00aa\3\2\2\2\u00ad\u00ae\7\21\2\2\u00ae\u00af\7\35\2\2\u00af\31\3\2"+
		"\2\2\u00b0\u00b1\7\22\2\2\u00b1\u00b2\7\35\2\2\u00b2\33\3\2\2\2\u00b3"+
		"\u00b4\t\2\2\2\u00b4\35\3\2\2\2\u00b5\u00b6\7,\2\2\u00b6\37\3\2\2\2\u00b7"+
		"\u00b8\7\20\2\2\u00b8\u00bd\5\"\22\2\u00b9\u00ba\7$\2\2\u00ba\u00bc\5"+
		"\"\22\2\u00bb\u00b9\3\2\2\2\u00bc\u00bf\3\2\2\2\u00bd\u00bb\3\2\2\2\u00bd"+
		"\u00be\3\2\2\2\u00be!\3\2\2\2\u00bf\u00bd\3\2\2\2\u00c0\u00c3\5&\24\2"+
		"\u00c1\u00c3\5$\23\2\u00c2\u00c0\3\2\2\2\u00c2\u00c1\3\2\2\2\u00c3#\3"+
		"\2\2\2\u00c4\u00d1\5*\26\2\u00c5\u00d1\5P)\2\u00c6\u00d1\5,\27\2\u00c7"+
		"\u00d1\5L\'\2\u00c8\u00d1\5\62\32\2\u00c9\u00d1\5\64\33\2\u00ca\u00d1"+
		"\5\66\34\2\u00cb\u00d1\5@!\2\u00cc\u00d1\58\35\2\u00cd\u00d1\5<\37\2\u00ce"+
		"\u00d1\5:\36\2\u00cf\u00d1\5H%\2\u00d0\u00c4\3\2\2\2\u00d0\u00c5\3\2\2"+
		"\2\u00d0\u00c6\3\2\2\2\u00d0\u00c7\3\2\2\2\u00d0\u00c8\3\2\2\2\u00d0\u00c9"+
		"\3\2\2\2\u00d0\u00ca\3\2\2\2\u00d0\u00cb\3\2\2\2\u00d0\u00cc\3\2\2\2\u00d0"+
		"\u00cd\3\2\2\2\u00d0\u00ce\3\2\2\2\u00d0\u00cf\3\2\2\2\u00d1%\3\2\2\2"+
		"\u00d2\u00db\3\2\2\2\u00d3\u00db\5.\30\2\u00d4\u00db\5\60\31\2\u00d5\u00db"+
		"\5J&\2\u00d6\u00db\5B\"\2\u00d7\u00db\5N(\2\u00d8\u00db\5F$\2\u00d9\u00db"+
		"\5> \2\u00da\u00d2\3\2\2\2\u00da\u00d3\3\2\2\2\u00da\u00d4\3\2\2\2\u00da"+
		"\u00d5\3\2\2\2\u00da\u00d6\3\2\2\2\u00da\u00d7\3\2\2\2\u00da\u00d8\3\2"+
		"\2\2\u00da\u00d9\3\2\2\2\u00db\'\3\2\2\2\u00dc\u00dd\5\"\22\2\u00dd)\3"+
		"\2\2\2\u00de\u00df\7,\2\2\u00df\u00e0\7#\2\2\u00e0\u00e1\5\"\22\2\u00e1"+
		"\u00e2\7%\2\2\u00e2+\3\2\2\2\u00e3\u00e4\7\23\2\2\u00e4\u00e5\7\6\2\2"+
		"\u00e5\u00e6\7\24\2\2\u00e6-\3\2\2\2\u00e7\u00e8\7#\2\2\u00e8\u00e9\5"+
		"T+\2\u00e9\u00ea\7\t\2\2\u00ea\u00eb\5V,\2\u00eb\u00ec\7%\2\2\u00ec/\3"+
		"\2\2\2\u00ed\u00ee\5$\23\2\u00ee\u00ef\7\32\2\2\u00ef\u00f0\5X-\2\u00f0"+
		"\u00f1\7\t\2\2\u00f1\u00f2\5Z.\2\u00f2\61\3\2\2\2\u00f3\u00f4\7\27\2\2"+
		"\u00f4\63\3\2\2\2\u00f5\u00f6\7\35\2\2\u00f6\65\3\2\2\2\u00f7\u00f8\7"+
		"\36\2\2\u00f8\67\3\2\2\2\u00f9\u00fa\7!\2\2\u00fa9\3\2\2\2\u00fb\u00fc"+
		"\7\37\2\2\u00fc;\3\2\2\2\u00fd\u00fe\7 \2\2\u00fe=\3\2\2\2\u00ff\u0100"+
		"\5T+\2\u0100\u0101\7\33\2\2\u0101\u0102\5V,\2\u0102?\3\2\2\2\u0103\u0104"+
		"\7+\2\2\u0104A\3\2\2\2\u0105\u0106\t\3\2\2\u0106C\3\2\2\2\u0107\u0108"+
		"\7#\2\2\u0108\u0109\5T+\2\u0109\u010a\7\n\2\2\u010a\u010b\5V,\2\u010b"+
		"\u010c\7%\2\2\u010cE\3\2\2\2\u010d\u010e\5$\23\2\u010e\u010f\5\\/\2\u010f"+
		"\u0110\5$\23\2\u0110G\3\2\2\2\u0111\u0112\7,\2\2\u0112\u0113\7\b\2\2\u0113"+
		"\u0114\7,\2\2\u0114I\3\2\2\2\u0115\u0116\7\13\2\2\u0116\u0117\7#\2\2\u0117"+
		"\u0118\5\"\22\2\u0118\u0119\7%\2\2\u0119K\3\2\2\2\u011a\u011b\7+\2\2\u011b"+
		"M\3\2\2\2\u011c\u011d\5$\23\2\u011d\u011e\7\5\2\2\u011e\u011f\7#\2\2\u011f"+
		"\u0124\5\"\22\2\u0120\u0121\7$\2\2\u0121\u0123\5\"\22\2\u0122\u0120\3"+
		"\2\2\2\u0123\u0126\3\2\2\2\u0124\u0122\3\2\2\2\u0124\u0125\3\2\2\2\u0125"+
		"\u0127\3\2\2\2\u0126\u0124\3\2\2\2\u0127\u0128\7%\2\2\u0128O\3\2\2\2\u0129"+
		"\u012a\7#\2\2\u012a\u012b\5T+\2\u012b\u012c\7-\2\2\u012c\u012d\5V,\2\u012d"+
		"\u012e\7%\2\2\u012eQ\3\2\2\2\u012f\u0130\7\r\2\2\u0130\u0131\5\"\22\2"+
		"\u0131S\3\2\2\2\u0132\u0133\5$\23\2\u0133U\3\2\2\2\u0134\u0135\5$\23\2"+
		"\u0135W\3\2\2\2\u0136\u0137\5$\23\2\u0137Y\3\2\2\2\u0138\u0139\5$\23\2"+
		"\u0139[\3\2\2\2\u013a\u013b\t\4\2\2\u013b]\3\2\2\2\21`cfilo|\u0086\u009e"+
		"\u00aa\u00bd\u00c2\u00d0\u00da\u0124";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}