using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CRoaring
{
    /// <summary>
    /// Manages a dynamic vector of bits.    
    /// </summary>
    [ComVisible(true)]
    [Serializable]
    public sealed class Bitmap //: IEnumerable<bool>
    {
        #region Fields
        private RoaringBitmap _roaringBitmap;
        private int _version;

        private KeyValuePair<int, bool> _cachedHasValue = new KeyValuePair<int, bool>(-1, false);   // <version, HasValue>
        private KeyValuePair<int, bool> _cachedHasOnlyOneValue = new KeyValuePair<int, bool>(-1, false);   // <version, HasOnlyOneValue>
        private KeyValuePair<int, long> _cachedPopulationCount = new KeyValuePair<int, long>(-1, 0);    // <version, PopulationCount>
        private KeyValuePair<int, long[]> _cachedValues = new KeyValuePair<int, long[]>(-1, null);   // <version, Values>
        private KeyValuePair<int, int[]> _cachedIntValues = new KeyValuePair<int, int[]>(-1, null);   // <version, Values>
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class.
        /// Allocates space to hold length bit values. All of the values in the bit array are set to false.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">length is less than zero.</exception>
        public Bitmap(long length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));

            _roaringBitmap = new RoaringBitmap((uint)length);
            _version = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="initialValue">The initial value for all bits.</param>
        public Bitmap(long length, bool initialValue)
        {
            if (initialValue)
                _roaringBitmap = RoaringBitmap.FromRange(0, (uint)length);
            else
                _roaringBitmap = new RoaringBitmap((uint)length);

            _version = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap" /> class.
        /// </summary>
        /// <param name="another">Another object to copy from.</param>
        /// <exception cref="System.ArgumentNullException">another</exception>
        public Bitmap(Bitmap another)
        {
            if (another == null)
                throw new ArgumentNullException("another");

            _roaringBitmap = another._roaringBitmap.Clone();
            _version = 0;
        }

        public Bitmap(RoaringBitmap roaringBitmap)
        {
            _roaringBitmap = roaringBitmap;
        }
        #endregion Constructors

        #region Properties
        public bool HasValue
        {
            get
            {
                if (_cachedHasValue.Key == _version)
                    return _cachedHasValue.Value;

                //TODO: Optimize, IsEmpty is incorrect correctly
                bool result = _roaringBitmap.Cardinality > 0;

                _cachedHasValue = new KeyValuePair<int, bool>(_version, result);

                return result;
            }
        }

        public bool HasOnlyOneValue
        {
            get
            {
                if (_cachedHasOnlyOneValue.Key == _version)
                    return _cachedHasOnlyOneValue.Value;

                var result = _roaringBitmap.Cardinality == 1;

                _cachedHasOnlyOneValue = new KeyValuePair<int, bool>(_version, result);

                return result;
            }
        }

        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public long[] Values
        {
            get
            {
                if (_cachedValues.Key == _version)
                    return _cachedValues.Value;

                long[] values = new long[PopulationCount];
                int i = 0;

                foreach (var item in _roaringBitmap)
                {
                    values[i++] = item;
                }

                _cachedValues = new KeyValuePair<int, long[]>(_version, values);

                return values;
            }
        }

        public int[] IntValues
        {
            get
            {
                if (_cachedIntValues.Key == _version)
                    return _cachedIntValues.Value;

                int[] values = new int[PopulationCount];
                int i = 0;

                foreach (var item in _roaringBitmap)
                {
                    values[i++] = (int)item;
                }

                _cachedIntValues = new KeyValuePair<int, int[]>(_version, values);

                return values;
            }
        }

        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public long PopulationCount
        {
            get
            {
                if (_cachedPopulationCount.Key == _version)
                    return _cachedPopulationCount.Value;

                long count = (long)_roaringBitmap.Cardinality;

                _cachedPopulationCount = new KeyValuePair<int, long>(_version, count);

                return count;
            }
        }

        public bool this[long index]
        {
            get { return Get(index); }
            set { Set(index, value); }
        }

        public int Version
        {
            get { return _version; }
        }
        #endregion Properties

        #region Internal Methods
        private static string GetResourceString(string s)
        {
            //TODO: Localization
            return s;
        }
        #endregion Internal Methods

        #region Public Methods
        /// <summary>
        /// Returns the bit value at position index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">index is less than zero or greater than the number of elements.</exception>
        public bool Get(long index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", GetResourceString("ArgumentOutOfRange_Index"));

            return _roaringBitmap.Contains((uint)index);
        }

        /// <summary>
        /// Returns if any bit value is 1 from start index to end index.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        public bool GetAny(long startIndex, long endIndex, long step)
        {
            for (long i = startIndex; i <= endIndex; i += step)
            {
                if (Get(i))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the bit value at position index to value.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index is less than zero.</exception>
        public void Set(long index, bool value)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", GetResourceString("ArgumentOutOfRange_Index"));

            if (value)
                _roaringBitmap.Add((uint)index);
            else
                _roaringBitmap.Remove((uint)index);

            _version++;
        }

        /// <summary>
        /// Sets the bit value at specified range to value.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index</exception>
        public void Set(long startIndex, long endIndex, bool value)
        {
            for (long i = startIndex; i <= endIndex; i++)
            {
                Set(i, value);
            }
        }

        public static Bitmap And(Bitmap array1, Bitmap array2)
        {
            if (array1 == null)
                return array2;

            if (array2 == null)
                return array1;

            return new Bitmap(array1._roaringBitmap.And(array2._roaringBitmap));            
        }

        /// <summary>
        /// Returns a reference to the current instance ANDed with value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Bitmap And(Bitmap value)
        {
            if (value == null)
                return this;

            _roaringBitmap.IAnd(value._roaringBitmap);

            _version++;
            return this;
        }

        /// <summary>
        /// Excludes the specified bits.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Bitmap Exclude(Bitmap value)
        {
            if (value == null)
                return this;

            _roaringBitmap.IAndNot(value._roaringBitmap);

            _version++;
            return this;
        }

        public static Bitmap Or(Bitmap array1, Bitmap array2)
        {
            if (array1 == null)
                return array2;

            if (array2 == null)
                return array1;

            return new Bitmap(array1._roaringBitmap.Or(array2._roaringBitmap));            
        }

        /// <summary>
        /// Returns a reference to the current instance ORed with value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">value is null.</exception>
        public Bitmap Or(Bitmap value)
        {
            if (value == null)
                return this;

            _roaringBitmap.IOr(value._roaringBitmap);

            _version++;
            return this;
        }

        //public static Bitmap Not(Bitmap value)
        //{
        //    if (value == null)
        //        return null;

        //    return new Bitmap(value._roaringBitmap.Not());
        //}

        ///// <summary>
        ///// Inverts all the bit values. On/true bit values are converted to 
        ///// off/false. Off/false bit values are turned on/true. The current instance
        ///// is updated and returned.
        ///// </summary>
        ///// <returns></returns>
        //public Bitmap Not()
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Determines whether [is superset of] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool IsSupersetOf(Bitmap value)
        {
            return Bitmap.AreEqual(Bitmap.And(this, value), value);
        }

        /// <summary>
        /// Determines whether [is subset of] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool IsSubsetOf(Bitmap value)
        {
            return Bitmap.AreEqual(Bitmap.And(this, value), this);
        }

        /// <summary>
        /// Determines if this instance and another instance overlaps.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool Overlaps(Bitmap value)
        {
            if (value == null)
                return false;

            //TODO: Improve
            return _roaringBitmap.And(value._roaringBitmap).Cardinality > 0;

            //bool isOverlapped = false;
            //var length = Math.Min(_array.LongLength, value._array.LongLength);

            //for (long i = 0; i < length; i++)
            //{
            //    if ((_array[i] & value._array[i]) != 0)
            //    {
            //        isOverlapped = true;
            //        break;
            //    }
            //}

            //return isOverlapped;
        }

        public static bool AreEqual(Bitmap array1, Bitmap array2)
        {
            if ((object)array1 == null)
                return (object)array2 == null;

            if ((object)array2 == null)
                return (object)array1 == null;

            return array1._roaringBitmap.Equals(array2._roaringBitmap);
        }

        public static bool operator ==(Bitmap a, Bitmap b)
        {
            return AreEqual(a, b);
        }

        public static bool operator !=(Bitmap a, Bitmap b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            var obj2 = obj as Bitmap;

            if ((object)obj2 == null)
                return false;

            return this == obj2;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            var count = PopulationCount;    // TODO: Improve

            hashCode |= (int)(count << 16);
                        
            // Use 32 bits as sample
            int index = 0;

            for (long i = 0, step = Math.Max(count / 32, 1); i < count; i += step)
            {
                if (this[i])
                    hashCode |= 1 << (int)(index % 32);

                index++;
            }

            return hashCode;
        }

        public static Bitmap Clone(Bitmap obj)
        {
            return obj == null ? null : new Bitmap(obj);
        }

        //public override string ToString()
        //{
        //    return "Length=" + Length;
        //}
        #endregion Public Methods
    }
}
