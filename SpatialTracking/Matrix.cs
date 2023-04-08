using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpatialTracking
{
	struct Matrix
	{
		private static readonly Random random = new Random();

		private readonly float[,] data;

		public int Rows { get; private set; }
		public int Cols { get; private set; }

		/// <summary>
		/// Create a matrix of specified size
		/// </summary>
		/// <param name="rows"></param>
		/// <param name="cols"></param>
		public Matrix(int rows, int cols, float defaultValue = 0f)
		{
			data = new float[rows, cols];
			Rows = rows;
			Cols = cols;

			InitializeWithValues(defaultValue);
		}

		/// <summary>
		/// Create a column vector
		/// </summary>
		/// <param name="rows"></param>
		public Matrix(int rows, float defaultValue = 0f)
		{
			data = new float[rows, 1];
			Rows = rows;
			Cols = 1;

			InitializeWithValues(defaultValue);
		}

		/// <summary>
		/// Create a matrix from the specified data.
		/// </summary>
		/// <param name="_data"></param>
		public Matrix(float[,] _data)
		{
			data = _data;
			Rows = data.GetLength(0);
			Cols = data.GetLength(1);
		}

		/// <summary>
		/// Create a matrix from the specified data.
		/// </summary>
		/// <param name="_data"></param>
		public Matrix(params float[] _data)
		{
			Rows = _data.Length;
			Cols = 1;
			data = new float[Rows, 1];
			for (int i = 0; i < _data.Length; i++)
			{
				data[i, 0] = _data[i];
			}
		}


		public float this[int row, int col]
		{
			get => data[row, col];
			set => data[row, col] = value;
		}

		public float this[int row]
		{
			get => data[row, 0];
			set => data[row, 0] = value;
		}

		public void Print(string prepend = "")
		{
			for (int i = 0; i < Rows; i++)
			{
				Console.Write(prepend);
				for (int j = 0; j < Cols; j++)
				{
					float datum = data[i, j];
					if (!float.IsNegative(datum))
					{
						Console.Write(" ");
					}
					Console.Write($"{datum:F3}, ");
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}

		public void Randomize(float min, float max)
		{
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] = (float)random.NextDouble() * (max - min) + min;
				}
			}
		}

		public Matrix Subtract(Matrix m)
		{
			if (m.Rows != Rows || m.Cols != Cols)
			{
				throw new Exception("ERROR: Tried to subtract matrixes of different size!");
			}
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] -= m[i, j];
				}
			}
			return this;
		}

		public Matrix Subtract(float n)
		{
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] -= n;
				}
			}
			return this;
		}

		public Matrix Add(Matrix m)
		{
			if (m.Rows != Rows || m.Cols != Cols)
			{
				throw new Exception("ERROR: Tried to add matrixes of different size!");
			}
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] += m[i, j];
				}
			}
			return this;
		}

		public Matrix AddScaled(Matrix m, float n)
		{
			if (m.Rows != Rows || m.Cols != Cols)
			{
				throw new Exception("ERROR: Tried to add matrixes of different size!");
			}
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] += m[i, j] * n;
				}
			}
			return this;
		}

		public Matrix Add(float n)
		{
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] += n;
				}
			}
			return this;
		}

		public Matrix Scale(float n)
		{
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] *= n;
				}
			}
			return this;
		}

		public void InitializeWithValues(float val)
		{
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] = val;
				}
			}
		}

		public void SetCopy(Matrix m)
		{
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] = m[i, j];
				}
			}
		}

		public void SetTransposedCopy(Matrix m)
		{
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] = m[j, i];
				}
			}
		}

		public Matrix Evaluate(Func<float, float> mapper)
		{
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] = mapper(data[i, j]);
				}
			}
			return this;
		}

		public void Multiply(Matrix m)
		{
			if (m.Cols != Rows)
			{
				throw new Exception("Tried to multiply unmultiplicable matrices in Matrix.Multiply!");
			}
			for (int i = 0; i < m.Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					for (int k = 0; k < m.Cols; k++)
					{
						data[i, j] += m[i, k] * data[k, j];
					}
				}
			}
		}

		public void HadamardProduct(Matrix b)
		{
			for (int i = 0; i < Rows; i++)
			{
				for (int j = 0; j < Cols; j++)
				{
					data[i, j] *= b[i, j];
				}
			}

		}

		public static Matrix HadamardProduct(Matrix a, Matrix b)
		{
			if (a.Rows != b.Rows || a.Cols != b.Cols)
			{
				throw new Exception("Tried to multiply unmultiplicable matrices in Matrix.HadamardProduct!");
			}

			Matrix res = new Matrix(a.Rows, a.Cols);

			for (int i = 0; i < res.Rows; i++)
			{
				for (int j = 0; j < res.Cols; j++)
				{
					res[i, j] = a[i, j] * b[i, j];
				}
			}

			return res;
		}

		public static Matrix Identity(int size)
		{
			Matrix res = new Matrix(size, size, 0f);
			for (int i = 0; i < size; i++)
			{
				res[i, i] = 1f;
			}
			return res;
		}

		public static Matrix Invert3x3(Matrix m)
		{
			if (m.Rows != 3 || m.Cols != 3)
			{
				throw new ArgumentException("Matrix supplied to Invert3x3 was not a 3x3 matrix!");
			}

			Matrix res = new Matrix(3, 3);
			float det = 0f;

			for (int i = 0; i < 3; i++)
			{
				det += m[0, i] * (m[1, (i + 1) % 3] * m[2, (i + 2) % 3] - m[1, (i + 2) % 3] * m[2, (i + 1) % 3]);
			}

			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					res[j, i] = ((m[(i + 1) % 3, (j + 1) % 3] * m[(i + 2) % 3, (j + 2) % 3]) - (m[(i + 1) % 3, (j + 2) % 3] * m[(i + 2) % 3, (j + 1) % 3])) / det;
				}
			}

			return res;
		}

		public static Matrix RandomOfSize((int rows, int cols) size, (float min, float max) range)
		{
			Matrix res = new Matrix(size.rows, size.cols);
			res.Randomize(range.min, range.max);
			return res;
		}

		public static Matrix Multiply(Matrix a, Matrix b)
		{
			if (a.Cols != b.Rows)
			{
				throw new Exception("Tried to multiply unmultiplicable matrices in Matrix.Multiply!");
			}
			Matrix res = new Matrix(a.Rows, b.Cols);
			for (int i = 0; i < res.Rows; i++)
			{
				for (int j = 0; j < res.Cols; j++)
				{
					res[i, j] = 0f;
					for (int k = 0; k < a.Cols; k++)
					{
						res[i, j] += a[i, k] * b[k, j];
					}
				}
			}
			return res;
		}

		public static Matrix Subtract(Matrix a, Matrix b)
		{
			if (a.Rows != b.Rows || a.Cols != b.Cols)
			{
				throw new Exception("ERROR: Tried to subtract matrixes of different size!");
			}
			Matrix res = new Matrix(a.Rows, a.Cols);
			for (int i = 0; i < a.Rows; i++)
			{
				for (int j = 0; j < a.Cols; j++)
				{
					res[i, j] = a[i, j] - b[i, j];
				}
			}
			return res;
		}

		public static Matrix Subtract(Matrix m, float n)
		{
			Matrix res = new Matrix(m.Rows, m.Cols);
			for (int i = 0; i < m.Rows; i++)
			{
				for (int j = 0; j < m.Cols; j++)
				{
					res[i, j] = m[i, j] - n;
				}
			}
			return res;
		}

		public static Matrix Add(Matrix a, Matrix b)
		{
			if (a.Rows != b.Rows || a.Cols != b.Cols)
			{
				throw new Exception("ERROR: Tried to add matrixes of different size!");
			}
			Matrix res = new Matrix(a.Rows, a.Cols);
			for (int i = 0; i < a.Rows; i++)
			{
				for (int j = 0; j < a.Cols; j++)
				{
					res[i, j] = a[i, j] + b[i, j];
				}
			}
			return res;
		}

		public static Matrix Add(Matrix m, float n)
		{
			Matrix res = new Matrix(m.Rows, m.Cols);
			for (int i = 0; i < m.Rows; i++)
			{
				for (int j = 0; j < m.Cols; j++)
				{
					res[i, j] = m[i, j] + n;
				}
			}
			return res;
		}

		public static Matrix Scale(Matrix m, float n)
		{
			Matrix res = new Matrix(m.Rows, m.Cols);
			for (int i = 0; i < m.Rows; i++)
			{
				for (int j = 0; j < m.Cols; j++)
				{
					res[i, j] = m[i, j] * n;
				}
			}
			return res;
		}

		public static Matrix Evaluate(Matrix m, Func<float, float> mapper)
		{
			Matrix res = new Matrix(m.Rows, m.Cols);
			for (int i = 0; i < m.Rows; i++)
			{
				for (int j = 0; j < m.Cols; j++)
				{
					res.data[i, j] = mapper(m.data[i, j]);
				}
			}
			return res;
		}

		public static Matrix Transpose(Matrix m)
		{
			Matrix res = new Matrix(m.Cols, m.Rows);
			for (int i = 0; i < m.Rows; i++)
			{
				for (int j = 0; j < m.Cols; j++)
				{
					res[j, i] = m[i, j];
				}
			}
			return res;
		}

		public static Matrix operator +(Matrix m)
		{
			return m;
		}

		public static Matrix operator -(Matrix m)
		{
			return m.Scale(-1f);
		}

		public static Matrix operator +(Matrix a, Matrix b)
		{
			return Add(a, b);
		}

		public static Matrix operator -(Matrix a, Matrix b)
		{
			return Subtract(a, b);
		}

		public static Matrix operator *(Matrix a, Matrix b)
		{
			return Multiply(a, b);
		}
	}
}