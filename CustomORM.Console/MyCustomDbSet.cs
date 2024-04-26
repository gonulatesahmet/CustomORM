using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqExample
{
	public class MyCustomDbSet<T> where T : class, IEntity
	{
		private SqlConnection _connection;
		public Query<T> Table { get; }
		public MyCustomDbSet()
		{
			Table = new Query<T>();
		}
		public static void GetConnectionString(string connectionString)
		{

		}
	}


	public class Query<T> : List<T> where T : class, IEntity
	{
		private List<(string, QueryPriority)> Queries { get; set; }

		private bool whereCalled = false;
		private bool joinCalled = false;
		private bool orderCalled = false;
		private bool groupCalled = false;


		public Query()
		{
			Queries = new()
			{
				($"SELECT * FROM [{typeof(T).Name}]", QueryPriority.Select)
			};
		}
		public Query<T> Join<J>(Expression<Func<T, string>> func) where J : class, IEntity
		{
			if (!joinCalled)
			{
				MemberExpression? memberExpression = (MemberExpression)func.Body;
				string propertyName = memberExpression.Member.Name;

				Queries.Add(($"INNER JOIN {typeof(T).Name} ON {typeof(T).Name}.{propertyName} = {typeof(J).Name}.Id", QueryPriority.Inner));
				return this;
			}
			throw new InvalidOperationException("Multiple Calls Join Method.");
		}

		public Query<T> Where(Expression<Func<T, bool>> predicate)
		{
			if (!whereCalled)
			{
				whereCalled = true;
				List<string> key = new();
				List<string> value = new();

				Queries.Add((EditWhereCommand(((LambdaExpression)predicate).Body.ToString()), QueryPriority.Where));
				return this;
			}
			throw new InvalidOperationException("Multiple Calls Where Method.");
		}

		public Query<T> Order(Expression<Func<T, string>> predicate)
		{
			if (!orderCalled)
			{
				MemberExpression? memberExpression = (MemberExpression)predicate.Body;
				string propertyName = memberExpression.Member.Name;

				var dataControl = Queries.FirstOrDefault(x => x.Item2 == QueryPriority.Order);
				if (dataControl != default)
					dataControl.Item1 += $", {propertyName}";
				else
					Queries.Add(($"ORDER BY {propertyName}", QueryPriority.Order));

				return this;
			}
			throw new InvalidOperationException("Multiple Calls Order Method.");
		}

		public Query<T> Group(Expression<Func<T, string>> predicate)
		{
			if (!groupCalled)
			{
				MemberExpression? memberExpression = (MemberExpression)predicate.Body;
				string propertyName = memberExpression.Member.Name;

				var dataControl = Queries.FirstOrDefault(x => x.Item2 == QueryPriority.Group);
				if (dataControl != default)
					dataControl.Item1 += $", {propertyName}";
				else
					Queries.Add(($"GROUP BY {propertyName}", QueryPriority.Group));

				return this;
			}
			throw new InvalidOperationException("Multiple Calls Group Method.");
		}

		public string CreateQuery()
		{
			string query = string.Empty;
			Queries.OrderBy(x => x.Item2);
			foreach (var item in Queries)
			{
				query += item.Item1 + " ";
			}
			return query;
		}


		private string EditWhereCommand(string command)
		{
			string whereQuery = "WHERE ";
			whereQuery += command;

			whereQuery = whereQuery
				.Replace("AndAlso", "AND")
				.Replace("OrElse", "OR")
				.Replace("True", "1")
				.Replace("False", "0")
				.Replace(".Equals", "=")
				.Replace(".Contains", "LIKE")
				.Replace("==", "=")
				.Replace("(", "")
				.Replace(")", "");

			return whereQuery;
		}
	}

	public enum QueryPriority
	{
		Select,
		Inner,
		Where,
		Group,
		Order,

	}
}
