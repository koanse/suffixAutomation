using System;
using System.Collections.Generic;
using System.Linq;

namespace SuffixAutomation
{
	class Program
	{
		// Состояние автомата
		public struct AutomationState
		{
			// Длина подстроки для состояния
			public int Length { get; set; }
			// Индекс предыдущего состояния
			public int Link { get; set; }
			// Словарь следующих состояний
			public Dictionary<char, int> Next { get; set; }

			public override string ToString()
			{
				return string.Format("Длина строки для состояния: {0}, Индекс предыдущего состояния: {1}, Следующие состояния: {2}",
					Length, Link, string.Join(",", Next.Select(x => string.Format("(Символ: {0}, Индекс состояния: {1})", x.Key, x.Value)).ToList()));
			}
		};

		const int maxLen = 100000;

		// Массив состояний автомата
		private static AutomationState[] states;
		// Массив с числами подстрок, заканчивающихся в вершинах (для подсчета числа вхождений строк)
		private static int[] counts;

		// Актуальный размер массивов states и counts
		private static int size;
		// Последний использованный индекс
		private static int lastIndex;
		

		private static List<Tuple<int, int>> vertexIndexesByLength;

		// Инициализация автомата
		private static void InitSuffixAutomation()
		{
			states = new AutomationState[maxLen];
			counts = new int[maxLen];
			size = lastIndex = 0;
			states[0].Length = 0;
			states[0].Link = -1;
			vertexIndexesByLength = new List<Tuple<int, int>>();
			for (var i = 0; i < states.Length; i++)
			{
				states[i].Next = new Dictionary<char, int>();
			}

			size++;
		}

		// Добавление очередногго символа к автомату
		static void AddCharacterToSuffixAutomation(char c)
		{
			var cur = size++;
			states[cur].Length = states[lastIndex].Length + 1;
			int p;
			for (p = lastIndex; p != -1 && states[p].Next.All(x => x.Key != c); p = states[p].Link)
				states[p].Next[c] = cur;
			if (p == -1)
			{
				states[cur].Link = 0;
			}
			else
			{
				var q = states[p].Next[c];
				if (states[p].Length + 1 == states[q].Length)
				{
					states[cur].Link = q;
					counts[cur] = 1;
					vertexIndexesByLength.Add(new Tuple<int, int>(states[cur].Length, cur));
				}
				else
				{
					var clone = size++;
					states[clone].Length = states[p].Length + 1;
					states[clone].Next = states[q].Next.ToDictionary(x => x.Key, x => x.Value);
					states[clone].Link = states[q].Link;
					for (; p != -1 && states[p].Next.ContainsKey(c) && states[p].Next[c] == q; p = states[p].Link)
						states[p].Next[c] = clone;
					states[q].Link = states[cur].Link = clone;

					counts[clone] = 0;
					vertexIndexesByLength.Add(new Tuple<int, int>(states[clone].Length, clone));
				}
			}
			lastIndex = cur;
		}

		// Заполнение массива counts для подсчета всех вхождений подстроки
		private static void CalculateAllOccurences(string pattern)
		{
			for (var i = 0; i < size; i++)
			{
				counts[i] = GetAllSubstrings(i).Count();
			}
		}

		private static List<string> GetAllSubstrings(int index)
		{
			var currentState = states[index];
			var result = new List<string>();
			foreach (var next in currentState.Next)
			{
				var nextIndex = next.Value;
				var nextChar = next.Key;

				var subStrings = GetAllSubstrings(nextIndex);

				foreach (var str in subStrings)
				{
					result.Add(nextChar + str);
				}
			}

			if (!result.Any())
			{
				result.Add(string.Empty);
			}

			return result.Distinct().ToList();
		}

		// Подсчет числа вхождений подстроки
		private static int GetAllOccurencesCount(string pattern)
		{
			var count = 0;
			var index = 0;
			var nextIndex = 0;
			var state = states[0];
			foreach (var c in pattern)
			{
				if (state.Next.TryGetValue(c, out nextIndex))
				{
					index = nextIndex;
					state = states[index];
				}
				else
				{
					break;
				}
			}

			return counts[index];
		}

		static void Main(string[] args)
		{
			Console.WriteLine("Построение суффиксного автомата и расчет всех вхождений подстроки P за O(|P|)");
			Console.WriteLine("Введите строку для построения автомата:");
			var mainString = Console.ReadLine() + Environment.NewLine;

			InitSuffixAutomation();
			foreach (var c in mainString)
			{
				AddCharacterToSuffixAutomation(c);
			}

			Console.WriteLine("Автомат построен");
			int index = 0;
			foreach (var state in states)
			{
				Console.WriteLine("Номер состояния: {0}, Описание состояния: ({1})", Array.IndexOf(states, state), state);
				if (index++ == size - 1)
				{
					break;
				}
			}

			Console.WriteLine("Введите подстроку для расчета числа вхождений:");
			var pattern = Console.ReadLine();
			CalculateAllOccurences(pattern);

			var count = GetAllOccurencesCount(pattern);
			//var substr = string.Join(Environment.NewLine, GetAllSubstrings(0));

			Console.WriteLine("Число вхождений подстроки в основную строку: {0}", count);
			Console.ReadLine();
		}
	}
}
