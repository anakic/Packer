// Generated from c:\Projects\Packer\PBI_Expr_Antlr\pbiqLexer.g4 by ANTLR 4.9.2
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class pbiqLexer extends Lexer {
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
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"FROM", "AS", "IN", "WITH", "NATIVEREFERENCENAME", "DOT", "AND", "OR", 
			"NOT", "WHERE", "ORDERBY", "ASCENDING", "DESCENDING", "SELECT", "SKIP_", 
			"TOP", "ANYVALUE", "DEFAULTVALUEOVERRIDESANCESTORS", "TRANSFORM", "VIA", 
			"NULL", "TRUE", "FALSE", "BETWEEN", "CONTAINS", "AS_", "INTEGER", "DECIMAL", 
			"DATE", "DATETIMESECOND", "DATETIME", "SINGLE_QUOTE", "LPAREN", "COMMA", 
			"RPAREN", "GT", "GTE", "LT", "LTE", "EQ", "STRING_LITERAL", "IDENTIFIER", 
			"BINARY_ARITHMETIC_OPERATOR", "WS"
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


	public pbiqLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "pbiqLexer.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2.\u017a\b\1\4\2\t"+
		"\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13"+
		"\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22\t\22"+
		"\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31\t\31"+
		"\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t \4!"+
		"\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t+\4"+
		",\t,\4-\t-\3\2\3\2\3\2\3\2\3\2\3\3\3\3\3\3\3\4\3\4\3\4\3\5\3\5\3\5\3\5"+
		"\3\5\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3"+
		"\6\3\6\3\6\3\6\3\7\3\7\3\b\3\b\3\b\3\b\3\t\3\t\3\t\3\n\3\n\3\n\3\n\3\13"+
		"\3\13\3\13\3\13\3\13\3\13\3\f\3\f\3\f\3\f\3\f\3\f\3\f\3\f\3\r\3\r\3\r"+
		"\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\16\3\16\3\16\3\16\3\16\3\16\3\16\3\16\3"+
		"\16\3\16\3\16\3\17\3\17\3\17\3\17\3\17\3\17\3\17\3\20\3\20\3\20\3\20\3"+
		"\20\3\21\3\21\3\21\3\21\3\22\3\22\3\22\3\22\3\22\3\22\3\22\3\22\3\22\3"+
		"\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3"+
		"\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3\23\3"+
		"\23\3\23\3\23\3\24\3\24\3\24\3\24\3\24\3\24\3\24\3\24\3\24\3\24\3\25\3"+
		"\25\3\25\3\25\3\26\3\26\3\26\3\26\3\26\3\27\3\27\3\27\3\27\3\27\3\30\3"+
		"\30\3\30\3\30\3\30\3\30\3\31\3\31\3\31\3\31\3\31\3\31\3\31\3\31\3\32\3"+
		"\32\3\32\3\32\3\32\3\32\3\32\3\32\3\32\3\33\3\33\3\33\3\34\6\34\u011b"+
		"\n\34\r\34\16\34\u011c\3\35\6\35\u0120\n\35\r\35\16\35\u0121\3\35\3\35"+
		"\7\35\u0126\n\35\f\35\16\35\u0129\13\35\3\36\3\36\3\36\3\36\3\36\3\36"+
		"\3\36\3\36\3\36\3\36\3\36\3\37\3\37\3\37\3\37\3\37\3\37\3\37\3\37\3\37"+
		"\3\37\3\37\3 \3 \3 \3 \3 \3 \3 \3 \3 \3 \3!\3!\3\"\3\"\3#\3#\3$\3$\3%"+
		"\3%\3&\3&\3&\3\'\3\'\3(\3(\3(\3)\3)\3*\3*\6*\u0161\n*\r*\16*\u0162\3*"+
		"\3*\3+\6+\u0168\n+\r+\16+\u0169\3+\3+\6+\u016e\n+\r+\16+\u016f\3+\5+\u0173"+
		"\n+\3,\3,\3-\3-\3-\3-\4\u0162\u016f\2.\3\3\5\4\7\5\t\6\13\7\r\b\17\t\21"+
		"\n\23\13\25\f\27\r\31\16\33\17\35\20\37\21!\22#\23%\24\'\25)\26+\27-\30"+
		"/\31\61\32\63\33\65\34\67\359\36;\37= ?!A\"C#E$G%I&K\'M(O)Q*S+U,W-Y.\3"+
		"\2\36\4\2HHhh\4\2TTtt\4\2QQqq\4\2OOoo\4\2CCcc\4\2UUuu\4\2KKkk\4\2PPpp"+
		"\4\2YYyy\4\2VVvv\4\2JJjj\4\2XXxx\4\2GGgg\4\2EEee\3\2\60\60\4\2FFff\4\2"+
		"DDdd\4\2[[{{\4\2IIii\4\2FFgg\4\2NNnn\4\2MMmm\4\2RRrr\4\2WWww\3\2\62;\6"+
		"\2\62;C\\aac|\5\2,-//\61\61\5\2\13\f\17\17\"\"\2\u0180\2\3\3\2\2\2\2\5"+
		"\3\2\2\2\2\7\3\2\2\2\2\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2"+
		"\2\21\3\2\2\2\2\23\3\2\2\2\2\25\3\2\2\2\2\27\3\2\2\2\2\31\3\2\2\2\2\33"+
		"\3\2\2\2\2\35\3\2\2\2\2\37\3\2\2\2\2!\3\2\2\2\2#\3\2\2\2\2%\3\2\2\2\2"+
		"\'\3\2\2\2\2)\3\2\2\2\2+\3\2\2\2\2-\3\2\2\2\2/\3\2\2\2\2\61\3\2\2\2\2"+
		"\63\3\2\2\2\2\65\3\2\2\2\2\67\3\2\2\2\29\3\2\2\2\2;\3\2\2\2\2=\3\2\2\2"+
		"\2?\3\2\2\2\2A\3\2\2\2\2C\3\2\2\2\2E\3\2\2\2\2G\3\2\2\2\2I\3\2\2\2\2K"+
		"\3\2\2\2\2M\3\2\2\2\2O\3\2\2\2\2Q\3\2\2\2\2S\3\2\2\2\2U\3\2\2\2\2W\3\2"+
		"\2\2\2Y\3\2\2\2\3[\3\2\2\2\5`\3\2\2\2\7c\3\2\2\2\tf\3\2\2\2\13k\3\2\2"+
		"\2\r\177\3\2\2\2\17\u0081\3\2\2\2\21\u0085\3\2\2\2\23\u0088\3\2\2\2\25"+
		"\u008c\3\2\2\2\27\u0092\3\2\2\2\31\u009a\3\2\2\2\33\u00a4\3\2\2\2\35\u00af"+
		"\3\2\2\2\37\u00b6\3\2\2\2!\u00bb\3\2\2\2#\u00bf\3\2\2\2%\u00c8\3\2\2\2"+
		"\'\u00e7\3\2\2\2)\u00f1\3\2\2\2+\u00f5\3\2\2\2-\u00fa\3\2\2\2/\u00ff\3"+
		"\2\2\2\61\u0105\3\2\2\2\63\u010d\3\2\2\2\65\u0116\3\2\2\2\67\u011a\3\2"+
		"\2\29\u011f\3\2\2\2;\u012a\3\2\2\2=\u0135\3\2\2\2?\u0140\3\2\2\2A\u014a"+
		"\3\2\2\2C\u014c\3\2\2\2E\u014e\3\2\2\2G\u0150\3\2\2\2I\u0152\3\2\2\2K"+
		"\u0154\3\2\2\2M\u0157\3\2\2\2O\u0159\3\2\2\2Q\u015c\3\2\2\2S\u015e\3\2"+
		"\2\2U\u0172\3\2\2\2W\u0174\3\2\2\2Y\u0176\3\2\2\2[\\\t\2\2\2\\]\t\3\2"+
		"\2]^\t\4\2\2^_\t\5\2\2_\4\3\2\2\2`a\t\6\2\2ab\t\7\2\2b\6\3\2\2\2cd\t\b"+
		"\2\2de\t\t\2\2e\b\3\2\2\2fg\t\n\2\2gh\t\b\2\2hi\t\13\2\2ij\t\f\2\2j\n"+
		"\3\2\2\2kl\t\t\2\2lm\t\6\2\2mn\t\13\2\2no\t\b\2\2op\t\r\2\2pq\t\16\2\2"+
		"qr\t\3\2\2rs\t\16\2\2st\t\2\2\2tu\t\16\2\2uv\t\3\2\2vw\t\16\2\2wx\t\t"+
		"\2\2xy\t\17\2\2yz\t\16\2\2z{\t\t\2\2{|\t\6\2\2|}\t\5\2\2}~\t\16\2\2~\f"+
		"\3\2\2\2\177\u0080\t\20\2\2\u0080\16\3\2\2\2\u0081\u0082\t\6\2\2\u0082"+
		"\u0083\t\t\2\2\u0083\u0084\t\21\2\2\u0084\20\3\2\2\2\u0085\u0086\t\4\2"+
		"\2\u0086\u0087\t\3\2\2\u0087\22\3\2\2\2\u0088\u0089\t\t\2\2\u0089\u008a"+
		"\t\4\2\2\u008a\u008b\t\13\2\2\u008b\24\3\2\2\2\u008c\u008d\t\n\2\2\u008d"+
		"\u008e\t\f\2\2\u008e\u008f\t\16\2\2\u008f\u0090\t\3\2\2\u0090\u0091\t"+
		"\16\2\2\u0091\26\3\2\2\2\u0092\u0093\t\4\2\2\u0093\u0094\t\3\2\2\u0094"+
		"\u0095\t\21\2\2\u0095\u0096\t\16\2\2\u0096\u0097\t\3\2\2\u0097\u0098\t"+
		"\22\2\2\u0098\u0099\t\23\2\2\u0099\30\3\2\2\2\u009a\u009b\t\6\2\2\u009b"+
		"\u009c\t\7\2\2\u009c\u009d\t\17\2\2\u009d\u009e\t\16\2\2\u009e\u009f\t"+
		"\t\2\2\u009f\u00a0\t\21\2\2\u00a0\u00a1\t\b\2\2\u00a1\u00a2\t\t\2\2\u00a2"+
		"\u00a3\t\24\2\2\u00a3\32\3\2\2\2\u00a4\u00a5\t\25\2\2\u00a5\u00a6\t\16"+
		"\2\2\u00a6\u00a7\t\7\2\2\u00a7\u00a8\t\17\2\2\u00a8\u00a9\t\16\2\2\u00a9"+
		"\u00aa\t\t\2\2\u00aa\u00ab\t\21\2\2\u00ab\u00ac\t\b\2\2\u00ac\u00ad\t"+
		"\t\2\2\u00ad\u00ae\t\24\2\2\u00ae\34\3\2\2\2\u00af\u00b0\t\7\2\2\u00b0"+
		"\u00b1\t\16\2\2\u00b1\u00b2\t\26\2\2\u00b2\u00b3\t\16\2\2\u00b3\u00b4"+
		"\t\17\2\2\u00b4\u00b5\t\13\2\2\u00b5\36\3\2\2\2\u00b6\u00b7\t\7\2\2\u00b7"+
		"\u00b8\t\27\2\2\u00b8\u00b9\t\b\2\2\u00b9\u00ba\t\30\2\2\u00ba \3\2\2"+
		"\2\u00bb\u00bc\t\13\2\2\u00bc\u00bd\t\4\2\2\u00bd\u00be\t\30\2\2\u00be"+
		"\"\3\2\2\2\u00bf\u00c0\t\6\2\2\u00c0\u00c1\t\t\2\2\u00c1\u00c2\t\23\2"+
		"\2\u00c2\u00c3\t\r\2\2\u00c3\u00c4\t\6\2\2\u00c4\u00c5\t\26\2\2\u00c5"+
		"\u00c6\t\31\2\2\u00c6\u00c7\t\16\2\2\u00c7$\3\2\2\2\u00c8\u00c9\t\21\2"+
		"\2\u00c9\u00ca\t\16\2\2\u00ca\u00cb\t\2\2\2\u00cb\u00cc\t\6\2\2\u00cc"+
		"\u00cd\t\31\2\2\u00cd\u00ce\t\26\2\2\u00ce\u00cf\t\13\2\2\u00cf\u00d0"+
		"\t\r\2\2\u00d0\u00d1\t\6\2\2\u00d1\u00d2\t\26\2\2\u00d2\u00d3\t\31\2\2"+
		"\u00d3\u00d4\t\16\2\2\u00d4\u00d5\t\4\2\2\u00d5\u00d6\t\r\2\2\u00d6\u00d7"+
		"\t\16\2\2\u00d7\u00d8\t\3\2\2\u00d8\u00d9\t\3\2\2\u00d9\u00da\t\b\2\2"+
		"\u00da\u00db\t\21\2\2\u00db\u00dc\t\16\2\2\u00dc\u00dd\t\7\2\2\u00dd\u00de"+
		"\t\6\2\2\u00de\u00df\t\t\2\2\u00df\u00e0\t\17\2\2\u00e0\u00e1\t\16\2\2"+
		"\u00e1\u00e2\t\7\2\2\u00e2\u00e3\t\13\2\2\u00e3\u00e4\t\4\2\2\u00e4\u00e5"+
		"\t\3\2\2\u00e5\u00e6\t\7\2\2\u00e6&\3\2\2\2\u00e7\u00e8\t\13\2\2\u00e8"+
		"\u00e9\t\3\2\2\u00e9\u00ea\t\6\2\2\u00ea\u00eb\t\t\2\2\u00eb\u00ec\t\7"+
		"\2\2\u00ec\u00ed\t\2\2\2\u00ed\u00ee\t\4\2\2\u00ee\u00ef\t\3\2\2\u00ef"+
		"\u00f0\t\5\2\2\u00f0(\3\2\2\2\u00f1\u00f2\t\r\2\2\u00f2\u00f3\t\b\2\2"+
		"\u00f3\u00f4\t\6\2\2\u00f4*\3\2\2\2\u00f5\u00f6\t\t\2\2\u00f6\u00f7\t"+
		"\31\2\2\u00f7\u00f8\t\26\2\2\u00f8\u00f9\t\26\2\2\u00f9,\3\2\2\2\u00fa"+
		"\u00fb\t\13\2\2\u00fb\u00fc\t\3\2\2\u00fc\u00fd\t\31\2\2\u00fd\u00fe\t"+
		"\16\2\2\u00fe.\3\2\2\2\u00ff\u0100\t\2\2\2\u0100\u0101\t\6\2\2\u0101\u0102"+
		"\t\26\2\2\u0102\u0103\t\7\2\2\u0103\u0104\t\16\2\2\u0104\60\3\2\2\2\u0105"+
		"\u0106\t\22\2\2\u0106\u0107\t\16\2\2\u0107\u0108\t\13\2\2\u0108\u0109"+
		"\t\n\2\2\u0109\u010a\t\16\2\2\u010a\u010b\t\16\2\2\u010b\u010c\t\t\2\2"+
		"\u010c\62\3\2\2\2\u010d\u010e\t\17\2\2\u010e\u010f\t\4\2\2\u010f\u0110"+
		"\t\t\2\2\u0110\u0111\t\13\2\2\u0111\u0112\t\6\2\2\u0112\u0113\t\b\2\2"+
		"\u0113\u0114\t\t\2\2\u0114\u0115\t\7\2\2\u0115\64\3\2\2\2\u0116\u0117"+
		"\t\6\2\2\u0117\u0118\t\7\2\2\u0118\66\3\2\2\2\u0119\u011b\t\32\2\2\u011a"+
		"\u0119\3\2\2\2\u011b\u011c\3\2\2\2\u011c\u011a\3\2\2\2\u011c\u011d\3\2"+
		"\2\2\u011d8\3\2\2\2\u011e\u0120\t\32\2\2\u011f\u011e\3\2\2\2\u0120\u0121"+
		"\3\2\2\2\u0121\u011f\3\2\2\2\u0121\u0122\3\2\2\2\u0122\u0123\3\2\2\2\u0123"+
		"\u0127\7\60\2\2\u0124\u0126\t\32\2\2\u0125\u0124\3\2\2\2\u0126\u0129\3"+
		"\2\2\2\u0127\u0125\3\2\2\2\u0127\u0128\3\2\2\2\u0128:\3\2\2\2\u0129\u0127"+
		"\3\2\2\2\u012a\u012b\t\32\2\2\u012b\u012c\t\32\2\2\u012c\u012d\t\32\2"+
		"\2\u012d\u012e\t\32\2\2\u012e\u012f\7/\2\2\u012f\u0130\t\32\2\2\u0130"+
		"\u0131\t\32\2\2\u0131\u0132\7/\2\2\u0132\u0133\t\32\2\2\u0133\u0134\t"+
		"\32\2\2\u0134<\3\2\2\2\u0135\u0136\5;\36\2\u0136\u0137\7V\2\2\u0137\u0138"+
		"\t\32\2\2\u0138\u0139\t\32\2\2\u0139\u013a\7<\2\2\u013a\u013b\t\32\2\2"+
		"\u013b\u013c\t\32\2\2\u013c\u013d\7<\2\2\u013d\u013e\t\32\2\2\u013e\u013f"+
		"\t\32\2\2\u013f>\3\2\2\2\u0140\u0141\5=\37\2\u0141\u0142\7\60\2\2\u0142"+
		"\u0143\t\32\2\2\u0143\u0144\t\32\2\2\u0144\u0145\t\32\2\2\u0145\u0146"+
		"\t\32\2\2\u0146\u0147\t\32\2\2\u0147\u0148\t\32\2\2\u0148\u0149\t\32\2"+
		"\2\u0149@\3\2\2\2\u014a\u014b\7)\2\2\u014bB\3\2\2\2\u014c\u014d\7*\2\2"+
		"\u014dD\3\2\2\2\u014e\u014f\7.\2\2\u014fF\3\2\2\2\u0150\u0151\7+\2\2\u0151"+
		"H\3\2\2\2\u0152\u0153\7@\2\2\u0153J\3\2\2\2\u0154\u0155\7@\2\2\u0155\u0156"+
		"\7?\2\2\u0156L\3\2\2\2\u0157\u0158\7>\2\2\u0158N\3\2\2\2\u0159\u015a\7"+
		">\2\2\u015a\u015b\7?\2\2\u015bP\3\2\2\2\u015c\u015d\7?\2\2\u015dR\3\2"+
		"\2\2\u015e\u0160\7$\2\2\u015f\u0161\13\2\2\2\u0160\u015f\3\2\2\2\u0161"+
		"\u0162\3\2\2\2\u0162\u0163\3\2\2\2\u0162\u0160\3\2\2\2\u0163\u0164\3\2"+
		"\2\2\u0164\u0165\7$\2\2\u0165T\3\2\2\2\u0166\u0168\t\33\2\2\u0167\u0166"+
		"\3\2\2\2\u0168\u0169\3\2\2\2\u0169\u0167\3\2\2\2\u0169\u016a\3\2\2\2\u016a"+
		"\u0173\3\2\2\2\u016b\u016d\7]\2\2\u016c\u016e\13\2\2\2\u016d\u016c\3\2"+
		"\2\2\u016e\u016f\3\2\2\2\u016f\u0170\3\2\2\2\u016f\u016d\3\2\2\2\u0170"+
		"\u0171\3\2\2\2\u0171\u0173\7_\2\2\u0172\u0167\3\2\2\2\u0172\u016b\3\2"+
		"\2\2\u0173V\3\2\2\2\u0174\u0175\t\34\2\2\u0175X\3\2\2\2\u0176\u0177\t"+
		"\35\2\2\u0177\u0178\3\2\2\2\u0178\u0179\b-\2\2\u0179Z\3\2\2\2\n\2\u011c"+
		"\u0121\u0127\u0162\u0169\u016f\u0172\3\2\3\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}