using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

class Program {
	static void Main(string[] args) {
		Console.WriteLine("Hello World!");

		var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

		var context = new MyDbContext(loggerFactory);
		context.Database.EnsureCreated();
		InitializeData(context);

		Console.WriteLine("All posts:");
		var data = context.BlogPosts.Select(x => x.Title).ToList();
		Console.WriteLine(JsonSerializer.Serialize(data));



		Console.WriteLine("How many comments each user left:");
		//ToDo: write a query and dump the data to console
		// Expected result (format could be different, e.g. object serialized to JSON is ok):
		// Ivan: 4
		// Petr: 2
		// Elena: 3

		///<summary>
		/// FIRST QUERY
		///</summary>
		var comments = context.BlogComments
			.GroupBy(c => c.UserName)
			.Select(c => new { name = c.Key, count = c.Count() })
			.ToList();
		comments.ForEach(x => Console.WriteLine($"{x.name}: {x.count}"));


		Console.WriteLine("Posts ordered by date of last comment. Result should include text of last comment:");
		//ToDo: write a query and dump the data to console
		// Expected result (format could be different, e.g. object serialized to JSON is ok):
		// Post2: '2020-03-06', '4'
		// Post1: '2020-03-05', '8'
		// Post3: '2020-02-14', '9'

		///<summary>
		/// SECOND QUERY
		///</summary>
		var postsByLastComment = context.BlogPosts
			.Select(b => new {
				name = b.Title,
				date = b.Comments.Max(c => c.CreatedDate).Date,
				text = b.Comments.OrderByDescending(c => c.CreatedDate).First().Text
			})
			.OrderByDescending(x => x.date)
			.ToList();

		postsByLastComment.ForEach(x => Console.WriteLine($"{x.name}: {x.date.ToShortDateString()}, {x.text}"));


		Console.WriteLine("How many last comments each user left:");
		// 'last comment' is the latest Comment in each Post
		//ToDo: write a query and dump the data to console
		// Expected result (format could be different, e.g. object serialized to JSON is ok):
		// Ivan: 2
		// Petr: 1

		///<summary>
		/// THIRD QUERY
		///</summary>
		var lastCommentsPerUser = context.BlogPosts
			.Select(b => b.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
			.GroupBy(x => x.UserName)
			.Select(x => new { name = x.Key, amount = x.Count() })
			.ToList();

		lastCommentsPerUser.ForEach(x => Console.WriteLine($"{x.name}: {x.amount}"));

		// Console.WriteLine(
		//     JsonSerializer.Serialize(BlogService.NumberOfCommentsPerUser(context)));
		// Console.WriteLine(
		//     JsonSerializer.Serialize(BlogService.PostsOrderedByLastCommentDate(context)));
		// Console.WriteLine(
		//     JsonSerializer.Serialize(BlogService.NumberOfLastCommentsLeftByUser(context)));

	}

	private static void InitializeData(MyDbContext context) {
		context.BlogPosts.Add(new BlogPost("Post1") {
			Comments = new List<BlogComment>()
			{
				new BlogComment("1", new DateTime(2020, 3, 2), "Petr"),
				new BlogComment("2", new DateTime(2020, 3, 4), "Elena"),
				new BlogComment("8", new DateTime(2020, 3, 5), "Ivan"),
			}
		});
		context.BlogPosts.Add(new BlogPost("Post2") {
			Comments = new List<BlogComment>()
			{
				new BlogComment("3", new DateTime(2020, 3, 5), "Elena"),
				new BlogComment("4", new DateTime(2020, 3, 6), "Ivan"),
			}
		});
		context.BlogPosts.Add(new BlogPost("Post3") {
			Comments = new List<BlogComment>()
			{
				new BlogComment("5", new DateTime(2020, 2, 7), "Ivan"),
				new BlogComment("6", new DateTime(2020, 2, 9), "Elena"),
				new BlogComment("7", new DateTime(2020, 2, 10), "Ivan"),
				new BlogComment("9", new DateTime(2020, 2, 14), "Petr"),
			}
		});
		context.SaveChanges();
	}
}