using BreadTh.StronglyApied.Attributes;

namespace BreadTh.StronglyApied.Samples
{
    [StronglyApiedRoot(DataModel.XML)]
    public class ExampleXmlModel 
    {
        [StronglyApiedObject()] public StringChildren stringChildren;
        [StronglyApiedObject()] public IntegerChildren integerChildren;
        [StronglyApiedObject()] public DecimalChildren decimalChildren;
        [StronglyApiedObject()] public OptionChildren optionChildren;
        [StronglyApiedObject()] public ObjectChildren objectChildren;
        [StronglyApiedObject()] public ListChildren listChildren;
        [StronglyApiedObject()] public StringAttributes stringAttributes;
        [StronglyApiedObject()] public IntegerAttributes integerAttributes;
        [StronglyApiedObject()] public DecimalAttributes decimalAttributes;
        [StronglyApiedObject()] public OptionAttributes optionAttributes;
        [StronglyApiedObject()] public ListAttributes listAttributes;

        public class ListAttributes
        {
            [StronglyApiedChild("item"), StronglyApiedArray(), StronglyApiedObject()] public Item[] items;

            public class Item
            {
                [StronglyApiedAttribute(), StronglyApiedString()] public string field;
            }
        }

        public class StringChildren 
        {
            [StronglyApiedString()]               public string good1;
            [StronglyApiedString()]               public string good2;
            [StronglyApiedString()]               public string bad1;
            [StronglyApiedString()]               public string bad2;
            [StronglyApiedString()]               public string bad3;
            [StronglyApiedString(optional: true)] public string goodOptional1;
            [StronglyApiedString(optional: true)] public string goodOptional2;
        }

        public class IntegerChildren 
        {
            [StronglyApiedLong()]               public long good1;
            [StronglyApiedLong()]               public long bad1;
            [StronglyApiedLong()]               public long bad2;
            [StronglyApiedLong()]               public long bad3;
            [StronglyApiedLong()]               public long bad4;
            [StronglyApiedLong()]               public long bad5;
            [StronglyApiedLong(optional: true)] public long? goodOptional1;
            [StronglyApiedLong(optional: true)] public long? goodOptional2;
        }

        public class DecimalChildren 
        {
            [StronglyApiedDecimal(minDecimalDigits: 1)] public decimal good1;
            [StronglyApiedDecimal(minDecimalDigits: 1)] public decimal good2;
            [StronglyApiedDecimal()]                    public decimal good3;
            [StronglyApiedDecimal(minDecimalDigits: 1)] public decimal bad1;
            [StronglyApiedDecimal()]                    public decimal bad2;
            [StronglyApiedDecimal()]                    public decimal bad3;
            [StronglyApiedDecimal()]                    public decimal bad4;
            [StronglyApiedDecimal()]                    public decimal bad5;
            [StronglyApiedDecimal()]                    public decimal bad6;
            [StronglyApiedDecimal(optional: true)]      public decimal goodOptional1;
            [StronglyApiedDecimal(optional: true)]      public decimal goodOptional2;
        }

        public class OptionChildren 
        {
            public enum OptionValues { Undefined, Option1, Option2 }

            [StronglyApiedOption()]               public OptionValues good1;
            [StronglyApiedOption()]               public OptionValues good2;
            [StronglyApiedOption()]               public OptionValues bad1;
            [StronglyApiedOption()]               public OptionValues bad2;
            [StronglyApiedOption()]               public OptionValues bad3;
            [StronglyApiedOption()]               public OptionValues bad4;
            [StronglyApiedOption()]               public OptionValues bad5;
            [StronglyApiedOption(optional: true)] public OptionValues goodOptional1;
            [StronglyApiedOption(optional: true)] public OptionValues goodOptional2;
        }

        public class ObjectChildren 
        {
            [StronglyApiedObject()]               public OneField good1;
            [StronglyApiedObject()]               public TwoField good2;
            [StronglyApiedObject()]               public OneField bad1;
            [StronglyApiedObject()]               public OneField bad2;
            [StronglyApiedObject()]               public OneField bad3;
            [StronglyApiedObject(optional: true)] public OneField goodOptional;

            public class OneField
            {
                [StronglyApiedString()] public string field;
            }

            public class TwoField 
            {
                [StronglyApiedString()] public string field1;
                [StronglyApiedString()] public string field2;
            } 
        }

        public class ListChildren 
        {
            [StronglyApiedArray(), StronglyApiedLong()]               public long[] good;
            [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad1;
            [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad2;
            [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad3;
            [StronglyApiedArray(), StronglyApiedLong()]               public long[] bad4;
            [StronglyApiedArray(), StronglyApiedLong(optional: true)] public long?[] goodOptional1;
            [StronglyApiedArray(optional: true), StronglyApiedLong()] public long[] goodOptional2;
        }

        public class StringAttributes
        {
            [StronglyApiedAttribute(), StronglyApiedString()]               public string good;
            [StronglyApiedAttribute(), StronglyApiedString()]               public string bad;
            [StronglyApiedAttribute(), StronglyApiedString(optional: true)] public string goodOptional;
        }

        public class IntegerAttributes
        {
            [StronglyApiedAttribute(), StronglyApiedLong()]               public long good;
            [StronglyApiedAttribute(), StronglyApiedLong()]               public long bad;
            [StronglyApiedAttribute(), StronglyApiedLong(optional: true)] public long? goodOptional;
        }

        public class DecimalAttributes
        {
            [StronglyApiedAttribute(), StronglyApiedDecimal()]               public decimal good;
            [StronglyApiedAttribute(), StronglyApiedDecimal()]               public decimal bad;
            [StronglyApiedAttribute(), StronglyApiedDecimal(optional: true)] public decimal goodOptional;
        }

        public class OptionAttributes
        {
            public enum Values { Undefined, Option1, Option2 }

            [StronglyApiedAttribute(), StronglyApiedOption()]               public Values good;
            [StronglyApiedAttribute(), StronglyApiedOption()]               public Values bad;
            [StronglyApiedAttribute(), StronglyApiedOption(optional: true)] public Values goodOptional;
        }

        public const string exampleInput =
        @"<root>
            <stringChildren>
                <good1>hello</good1>
                <good2>10</good2>
                <good3></good3>
                <bad1><field>10</field></bad1>
                <bad2><field>10</field><field>20</field></bad2>
                <goodOptional1></goodOptional1>
            </stringChildren>
            <integerChildren>
                <good1>10</good1>
                <bad1>abc</bad1>
                <bad2><field>10</field></bad2>
                <bad3><field>10</field><field>20</field></bad3>
                <bad4></bad4>
                <goodOptional1></goodOptional1>
            </integerChildren>
            <decimalChildren>
                <good1>12.34</good1>
                <good2>10.00</good2>
                <good3>10</good3>
                <bad1>10</bad1>
                <bad2>abc</bad2>
                <bad3><field>10.10</field></bad3>
                <bad4><field>10.00</field><field>20.00</field></bad4>
                <bad5></bad5>
                <goodOptional1></goodOptional1>
            </decimalChildren>
            <optionChildren>
                <good1>Option1</good1>
                <good2>Option2</good2>
                <bad1>Option4</bad1>
                <bad2>option1</bad2>
                <bad3></bad3>
                <bad4>Undefined</bad4>
                <goodOptional1></goodOptional1>
            </optionChildren>
            <objectChildren>
                <good1><field>value</field></good1>
                <good2><field1>value</field1><field2>value</field2></good2>
                <bad1>value</bad1>
                <bad2><field>value1</field><field>value2</field></bad2>
            </objectChildren>
            <listChildren>
                <good>1</good>
                <good>2</good>
                <good>3</good>
                <bad1>1</bad1>
                <bad1>2</bad1>
                <bad1>value</bad1>
                <bad2>1</bad2>
                <bad2>2</bad2>
                <bad2><field>3</field></bad2>
                <bad3>1</bad3>
                <bad3>2</bad3>
                <bad3><field>3</field><field>3</field></bad3>
                <bad4>1</bad4>
                <bad4>2</bad4>
                <bad4></bad4>
                <goodOptional1>1</goodOptional1>
                <goodOptional1>2</goodOptional1>
                <goodOptional1></goodOptional1>
            </listChildren>
            <stringAttributes good=""value""></stringAttributes>
            <integerAttributes good=""10""></integerAttributes>
            <decimalAttributes good=""10.00""></decimalAttributes>
            <optionAttributes good=""Option1""></optionAttributes>
            <listAttributes>
                <item field=""value1""></item>
                <item field=""value2""></item>
            </listAttributes>
        </root>";
    }
}
