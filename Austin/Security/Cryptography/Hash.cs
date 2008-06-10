using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;

namespace Austin.Security.Cryptography
{
	/// <summary>
	/// An abstract representation of a hash.
	/// </summary>
	public class Hash : IComparable, IComparable<Hash>, IEquatable<Hash>
	{
		#region Static Helpers
		private static object m_sync = new object();

		private static HashAlgorithm m_hashAlgorithm = MD5.Create();
		private static HashAlgorithm HashAlgorithm
		{
			get
			{
				return m_hashAlgorithm;
			}
		}

		private static Encoding m_encoding = Encoding.Unicode;
		private static Encoding Encoding
		{
			get
			{
				return m_encoding;
			}
		}
		#endregion

		#region Constructors and Data
		private byte[] m_hashedData;
		/// <summary>
		/// A hashed <see cref="System.Byte"/> array of the input data.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public byte[] HashedData
		{
			get
			{
				return this.m_hashedData;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Security.Cryptography.Hash"></see> class from the specified <see cref="System.Byte"/> array.
		/// </summary>
		/// <param name="input">The <see cref="System.Byte"/> array to be hashed.</param>
		public Hash(byte[] input)
		{
			this.m_hashedData = Hash.HashAlgorithm.ComputeHash(input);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Austin.Security.Cryptography.Hash"></see> class from the specified <see cref="System.String"/>.
		/// </summary>
		/// <param name="input">The <see cref="System.String"/> to be hashed.</param>
		public Hash(string input)
			: this(Hash.Encoding.GetBytes(input))
		{
		}
		#endregion

		#region Methods
		/// <summary>
		/// Compares this instance with a specified <see cref="Austin.Security.Cryptography.Hash"/> object.
		/// </summary>
		/// <param name="obj">A <see cref="Austin.Security.Cryptography.Hash"/>.</param>
		/// <returns>
		/// A 32-bit signed integer indicating the relationship between the two comparands.
		/// Value:Condition.
		/// Less:than zero This instance is less than strB.
		/// Zero:This instance is equal to strB.
		/// Greater than zero:This instance is greater than strB.-or- strB is null.
		/// </returns>
		public int CompareTo(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			if (obj.GetType() != typeof(Hash))
				throw new ArgumentException("Must by type of Hash.", "obj");

			return this.CompareTo((Hash)obj);
		}

		/// <summary>
		/// Compares this instance with a specified <see cref="Austin.Security.Cryptography.Hash"/> object.
		/// </summary>
		/// <param name="other">A <see cref="Austin.Security.Cryptography.Hash"/>.</param>
		/// <returns>
		/// A 32-bit signed integer indicating the relationship between the two comparands.
		/// Value:Condition.
		/// Less:than zero This instance is less than strB.
		/// Zero:This instance is equal to strB.
		/// Greater than zero:This instance is greater than strB.-or- strB is null.
		/// </returns>
		public int CompareTo(Hash other)
		{
			if (other == null)
				throw new ArgumentNullException("other");

			Hash h = other;
			byte[] bytes = h.HashedData;
			if (this.m_hashedData.Length != bytes.Length)
				return (this.m_hashedData.Length - bytes.Length);

			for (int i = 0; i < this.m_hashedData.Length; i++)
				if (this.m_hashedData[i] != bytes[i])
					return (this.m_hashedData[i] - bytes[i]);

			return 0;
		}

		/// <summary>
		/// Determines whether the specified <see cref="Austin.Security.Cryptography.Hash"/> has the same value of the current <see cref="Austin.Security.Cryptography.Hash"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Austin.Security.Cryptography.Hash"/> to compare with the current <see cref="Austin.Security.Cryptography.Hash"/>. </param>
		/// <returns>true if the value of the specified <see cref="Austin.Security.Cryptography.Hash"/> is the same as the current <see cref="Austin.Security.Cryptography.Hash"/>; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return (this.CompareTo(obj) == 0);
		}

		/// <summary>
		/// Determines whether the specified <see cref="Austin.Security.Cryptography.Hash"/> has the same value of the current <see cref="Austin.Security.Cryptography.Hash"/>.
		/// </summary>
		/// <param name="other">The <see cref="Austin.Security.Cryptography.Hash"/> to compare with the current <see cref="Austin.Security.Cryptography.Hash"/>.</param>
		/// <returns>true if the value of the specified <see cref="Austin.Security.Cryptography.Hash"/> is the same as the current <see cref="Austin.Security.Cryptography.Hash"/>; otherwise, false.</returns>
		public bool Equals(Hash other)
		{
			return (this.CompareTo(other) == 0);
		}

		/// <summary>
		/// Returns the hash code of the bytes of the hash.
		/// </summary>
		/// <returns>The hash code of the bytes of the hash.</returns>
		public override int GetHashCode()
		{
			int x = 1;
			foreach (byte b in this.m_hashedData)
				x = x ^ b;
			return x;
		}

		/// <summary>
		/// Creates a <see cref="System.String"/> representation of the hash.
		/// </summary>
		/// <returns>A <see cref="System.String"/> representation of the hash.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in this.m_hashedData)
			{
				sb.Append(b.ToString(CultureInfo.InvariantCulture));
				sb.Append("-");
			}
			sb.Remove(sb.Length - 1, 1);
			return sb.ToString();
		}
		#endregion

		#region Operators
		/// <summary>
		/// Determines whether the specified <see cref="Austin.Security.Cryptography.Hash"/> has a different value than the other <see cref="Austin.Security.Cryptography.Hash"/>.
		/// </summary>
		/// <param name="operand1">The first <see cref="Austin.Security.Cryptography.Hash"/> to compare.</param>
		/// <param name="operand2">The second <see cref="Austin.Security.Cryptography.Hash"/> to compare.</param>
		/// <returns>false if the value of the specified <see cref="Austin.Security.Cryptography.Hash"/> is the same as the current <see cref="Austin.Security.Cryptography.Hash"/>; otherwise, true.</returns>
		public static bool operator !=(Hash operand1, Hash operand2)
		{
			if (bothNull(operand1, operand2))
				return true;
			if (oneNull(operand1, operand2))
				return false;
			return !(operand1.Equals(operand2));
		}

		/// <summary>
		/// Determines whether the specified <see cref="Austin.Security.Cryptography.Hash"/> has the same value of the other <see cref="Austin.Security.Cryptography.Hash"/>.
		/// </summary>
		/// <param name="operand1">The first <see cref="Austin.Security.Cryptography.Hash"/> to compare.</param>
		/// <param name="operand2">The second <see cref="Austin.Security.Cryptography.Hash"/> to compare.</param>
		/// <returns>true if the value of the specified <see cref="Austin.Security.Cryptography.Hash"/> is the same as the other <see cref="Austin.Security.Cryptography.Hash"/>; otherwise, false.</returns>
		public static bool operator ==(Hash operand1, Hash operand2)
		{
			if (bothNull(operand1, operand2))
				return true;
			if (oneNull(operand1, operand2))
				return false;
			return operand1.Equals(operand2);
		}

		/// <summary>
		/// Determines whether <paramref name="operand1"/> if less than <paramref name="operand2"/>.
		/// </summary>
		/// <param name="operand1">The first <see cref="Austin.Security.Cryptography.Hash"/> to compare.</param>
		/// <param name="operand2">The first <see cref="Austin.Security.Cryptography.Hash"/> to compare.</param>
		/// <returns>true if the <paramref name="operand1"/> if less than <paramref name="operand2"/>, otherwise false.</returns>
		public static bool operator <(Hash operand1, Hash operand2)
		{
			if (bothNull(operand1, operand2))
				return true;
			if (oneNull(operand1, operand2))
				return false;
			return operand1.CompareTo(operand2) < 0;
		}

		/// <summary>
		/// Determines whether <paramref name="operand1"/> if greater than <paramref name="operand2"/>.
		/// </summary>
		/// <param name="operand1">The first <see cref="Austin.Security.Cryptography.Hash"/> to compare.</param>
		/// <param name="operand2">The first <see cref="Austin.Security.Cryptography.Hash"/> to compare.</param>
		/// <returns>true if the <paramref name="operand1"/> if greater than <paramref name="operand2"/>, otherwise false.</returns>
		public static bool operator >(Hash operand1, Hash operand2)
		{
			if (bothNull(operand1, operand2))
				return true;
			if (oneNull(operand1, operand2))
				return false;
			return operand1.CompareTo(operand2) > 0;
		}

		private static bool bothNull(Hash operand1, Hash operand2)
		{
			if (((object)operand1 == null) && ((object)operand2 == null))
				return true;
			return false;
		}

		private static bool oneNull(Hash operand1, Hash operand2)
		{
			if (((object)operand1 == null) || ((object)operand2 == null))
				return true;
			return false;
		}
		#endregion
	}
}
