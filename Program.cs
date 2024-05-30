using System.Text.Json;
using final_project_back_end_akostryba;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
    builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Basic Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

app.MapGet("/initialize", () =>
{

    using (var context = new ChirperContext())
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    string userJson = File.ReadAllText("userData.json");
    User[]? users = JsonSerializer.Deserialize<User[]>(userJson, options);

    string followerJson = File.ReadAllText("followerData.json");
    Follow[]? follows = JsonSerializer.Deserialize<Follow[]>(followerJson, options);

    string postJson = File.ReadAllText("postData.json");
    Post[]? posts = JsonSerializer.Deserialize<Post[]>(postJson, options);

    string commentJson = File.ReadAllText("commentData.json");
    Comment[]? comments = JsonSerializer.Deserialize<Comment[]>(commentJson, options);

    //Adding new data to tables
    using (var context = new ChirperContext())
    {
        //Create Users
        foreach (var user in users)
        {
            context.Users.Add(user);
        }

        context.SaveChanges();

        //Create Follows
        foreach (var follow in follows)
        {
            context.Follows.Add(follow);
        }

        context.SaveChanges();

        //Create Posts
        foreach (var post in posts)
        {
            context.Posts.Add(post);
        }

        context.SaveChanges();

        //Create Comments
        foreach (var comment in comments)
        {
            context.Comments.Add(comment);
        }

        context.SaveChanges();



        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }

}).WithName("Init").WithOpenApi();

app.MapGet("/users/{userId?}", (string? userId) =>{
    using (var context = new ChirperContext())
    {
        if(string.IsNullOrEmpty(userId) || userId==","){
            var users = context.Users.ToList();
            return users;
        }
        else{
            var matchingUser = context.Users.Where(u => u.userId == Int32.Parse(userId)).ToList();
            return matchingUser;
        }
    }
}).WithName("GetUsers").WithOpenApi();

app.MapGet("/following/{userId?}", (string? userId) =>
{
    using (var context = new ChirperContext())
    {
        if(string.IsNullOrEmpty(userId) || userId==","){
            var follows = context.Follows.ToList();
            return follows;
        }
        else{
            var matchingFollows = context.Follows.Where(f => f.followerId == Int32.Parse(userId)).ToList();
            return matchingFollows;
        }
    }
}).WithName("GetFollowing").WithOpenApi();

app.MapGet("/followers/{userId?}", (string? userId) =>
{
    using (var context = new ChirperContext())
    {
        if (string.IsNullOrEmpty(userId) || userId == ",")
        {
            var follows = context.Follows.ToList();
            return follows;
        }
        else
        {
            var matchingFollows = context.Follows.Where(f => f.followingId == Int32.Parse(userId)).ToList();
            return matchingFollows;
        }
    }
}).WithName("GetFollowers").WithOpenApi();

app.MapGet("/posts/{userId?}", (string? userId) =>
{
    using (var context = new ChirperContext())
    {
        if(string.IsNullOrEmpty(userId) || userId==","){
            var posts = context.Posts.ToList();
            return posts;
        }
        else{
            var matchingPosts = context.Posts.Where(p => p.userId == Int32.Parse(userId)).ToList();
            return matchingPosts;
        }
    }
}).WithName("GetPosts").WithOpenApi();

app.MapGet("/comments/{postId?}", (string? postId) =>
{
    using (var context = new ChirperContext())
    {
        if(string.IsNullOrEmpty(postId) || postId==","){
            var comments = context.Comments.ToList();
            return comments;
        }
        else{
            var matchingComments = context.Comments.Where(c => c.postId == Int32.Parse(postId)).ToList();
            return matchingComments;
        }
    }
}).WithName("GetComments").WithOpenApi();

app.MapPost("/user", (User user) =>
{
    using (var context = new ChirperContext())
    {
        context.Users.Add(user);
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }
    return Results.Created($"/user/{user.userId}", user);
}).WithName("PostUser").WithOpenApi();

app.MapPost("/follow", (Follow follow) =>
{
    using (var context = new ChirperContext())
    {
        context.Follows.Add(follow);
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }
    return Results.Created($"/follow/{follow.Id}", follow);
}).WithName("PostFollow").WithOpenApi().RequireAuthorization(new AuthorizeAttribute() { AuthenticationSchemes = "BasicAuthentication" }); ;

app.MapPost("/post", (Post post) =>
{
    using (var context = new ChirperContext())
    {
        context.Posts.Add(post);
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }
    return Results.Created($"/post/{post.postId}", post);
}).WithName("PostPost").WithOpenApi().RequireAuthorization(new AuthorizeAttribute() { AuthenticationSchemes = "BasicAuthentication" }); ;

app.MapPost("/comment", (Comment comment) =>
{
    using (var context = new ChirperContext())
    {
        context.Comments.Add(comment);
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }
    return Results.Created($"/comment/{comment.commentId}", comment);
}).WithName("PostComment").WithOpenApi().RequireAuthorization(new AuthorizeAttribute() { AuthenticationSchemes = "BasicAuthentication" }); ;

app.MapPut("/user/{userId}", (int userId, User updatedUser) =>
{
    using (var context = new ChirperContext())
    {
        var existingUser = context.Users.Find(userId);
        if (existingUser == null)
        {
            return Results.NotFound();
        }

        existingUser.username = updatedUser.username;
        existingUser.bio = updatedUser.bio;
        existingUser.profileImage = updatedUser.profileImage;

        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }
    return Results.Ok();
}).WithName("PutUser").WithOpenApi().RequireAuthorization(new AuthorizeAttribute() { AuthenticationSchemes = "BasicAuthentication" }); ;

app.MapDelete("/follow/{id}", (int id) =>
{
    using (var context = new ChirperContext())
    {
        var follow = context.Follows.Find(id);
        if (follow == null){
            return Results.NotFound();
        }
        context.Follows.Remove(follow);
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }    
    return Results.Ok();
}).WithName("DeleteUser").WithOpenApi().RequireAuthorization(new AuthorizeAttribute() { AuthenticationSchemes = "BasicAuthentication" }); ;

app.MapPost("/login", (User authenticatedUser) =>
{
    using (var context = new ChirperContext())
    {
        var user = context.Users.FirstOrDefault(l => l.username == authenticatedUser.username);
        if (user == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(user);
    }
}).WithName("Login").WithOpenApi().RequireAuthorization(new AuthorizeAttribute() { AuthenticationSchemes = "BasicAuthentication" });

app.MapPost("/newUser", (User newUser) =>
{
    
    using (var context = new ChirperContext())
    {
        context.Users.Add(newUser);
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }
    return Results.Created($"/newUser/{newUser.userId}", newUser);
}).WithName("PostLogin").WithOpenApi();

app.MapDelete("/deletePost/{postId}", (int postId) =>
{
    using (var context = new ChirperContext())
    {
        var post = context.Posts.Find(postId);
        if (post == null)
        {
            return Results.NotFound();
        }
        context.Posts.Remove(post);
        context.SaveChanges();
        context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint;");
    }
    return Results.Ok();
}).WithName("DeletePost").WithOpenApi();


app.Run();

