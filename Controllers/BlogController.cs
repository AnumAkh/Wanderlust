using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wanderlust.Models;

namespace Wanderlust.Controllers
{
    public class BlogController : Controller
    {
        private WanderlustEntities db = new WanderlustEntities();

        // GET: Blog
        public ActionResult Index()
        {
            // Check if the current user is an Author or a regular User
            if (Session["Role"] != null && Session["Role"].ToString() == "Author")
            {
                // For Authors: Only show their own blogs
                int authorId = Convert.ToInt32(Session["AuthorId"]);
                var authorBlogs = db.BLOGs
                    .Include(b => b.AUTHOR)
                    .Where(b => b.author_id == authorId)
                    .OrderByDescending(b => b.publication_date)
                    .ToList();

                // Return the Author's view of blogs (with edit/delete options)
                return View("AuthorIndex", authorBlogs);
            }
            else
            {
                // For regular users and visitors: Show all blogs
                var allBlogs = db.BLOGs
                    .Include(b => b.AUTHOR)
                    .OrderByDescending(b => b.publication_date)
                    .ToList();

                // Return the User's view of blogs (card display)
                return View("UserIndex", allBlogs);
            }
        }

        // GET: Blog/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BLOG blog = db.BLOGs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            // Check if user has liked this blog
            if (Session["UserId"] != null && Session["Role"] != null && Session["Role"].ToString() == "User")
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                var like = db.BLOG_LIKE.FirstOrDefault(l => l.blog_id == id && l.user_id == userId);

                ViewBag.UserLiked = (like != null);
            }

            // Get comments for this blog
            var comments = db.COMMENTs
                .Where(c => c.blog_id == id)
                .Include(c => c.USER)
                .OrderByDescending(c => c.date_posted)
                .ToList();

            ViewBag.Comments = comments;

            return View(blog);
        }

        // POST: Blog/ToggleLike
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleLike(int blogId)
        {
            // Check if user is logged in and is a regular user
            if (Session["UserId"] == null || Session["Role"] == null || Session["Role"].ToString() != "User")
            {
                return Json(new { success = false, message = "You must be logged in to like blogs." });
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            try
            {
                // Find the blog
                BLOG blog = db.BLOGs.Find(blogId);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Blog not found." });
                }

                // Check if user has already liked this blog
                var existingLike = db.BLOG_LIKE.FirstOrDefault(l => l.blog_id == blogId && l.user_id == userId);
                bool userLiked = false;

                if (existingLike != null)
                {
                    // Remove the like (toggle off)
                    db.BLOG_LIKE.Remove(existingLike);
                    // Update like count (make sure it doesn't go below 0)
                    blog.like_count = blog.like_count.HasValue ? Math.Max(0, blog.like_count.Value - 1) : 0;
                    userLiked = false;
                }
                else
                {
                    // Add a new like
                    var newLike = new BLOG_LIKE
                    {
                        blog_id = blogId,
                        user_id = userId,
                        like_date = DateTime.Now
                    };

                    db.BLOG_LIKE.Add(newLike);
                    // Update like count
                    blog.like_count = blog.like_count.HasValue ? blog.like_count.Value + 1 : 1;
                    userLiked = true;
                }

                // Save changes
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    likes = blog.like_count,
                    userLiked = userLiked
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // POST: Blog/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(int blogId, string commentContent)
        {
            // Check if user is logged in
            if (Session["UserId"] == null || Session["Role"] == null || Session["Role"].ToString() != "User")
            {
                return Json(new { success = false, message = "You must be logged in to comment on blogs." });
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            try
            {
                // Find the blog
                BLOG blog = db.BLOGs.Find(blogId);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Blog not found." });
                }

                // Create a new comment
                var newComment = new COMMENT
                {
                    blog_id = blogId,
                    user_id = userId,
                    content = commentContent,
                    date_posted = DateTime.Now
                };

                db.COMMENTs.Add(newComment);
                db.SaveChanges();

                // Get user info for the response
                var user = db.USERs.Find(userId);

                return Json(new
                {
                    success = true,
                    commentId = newComment.cmnt_id,
                    userName = user.firstName + " " + user.lastName,
                    commentDate = newComment.date_posted.ToString("MMM dd, yyyy h:mm tt"),
                    commentContent = newComment.content
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // GET: Blog/Create
        public ActionResult Create()
        {
            // Check if the user is an author
            if (Session["Role"] == null || Session["Role"].ToString() != "Author")
            {
                TempData["ErrorMessage"] = "You do not have permission to create blogs.";
                return RedirectToAction("Index", "Blog");
            }

            // Create a new blog with pre-filled author_id and publication_date
            var blog = new BLOG
            {
                author_id = Convert.ToInt32(Session["AuthorId"]),
                publication_date = DateTime.Now,

            };

            return View(blog);
        }

        // POST: Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "blog_id,title,content")] BLOG blog)
        {
            // Check if the user is an author
            if (Session["Role"] == null || Session["Role"].ToString() != "Author")
            {
                TempData["ErrorMessage"] = "You do not have permission to create blogs.";
                return RedirectToAction("Index", "Blog");
            }

            // Always set the author_id to the logged-in author's ID
            blog.author_id = Convert.ToInt32(Session["AuthorId"]);
            blog.publication_date = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.BLOGs.Add(blog);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(blog);
        }



        // GET: Blog/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Check if user is an author
            if (Session["Role"] == null || Session["Role"].ToString() != "Author")
            {
                TempData["ErrorMessage"] = "You do not have permission to edit blogs.";
                return RedirectToAction("Index", "Blog");
            }

            BLOG blog = db.BLOGs.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            // Check if this blog belongs to the current author
            int authorId = Convert.ToInt32(Session["AuthorId"]);
            if (blog.author_id != authorId)
            {
                TempData["ErrorMessage"] = "You can only edit your own blogs.";
                return RedirectToAction("Index", "Blog");
            }

            return View(blog);
        }

        //Edit Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "blog_id,author_id,title,content,publication_date")] BLOG blog)
        {
            // Check if user is an author
            if (Session["Role"] == null || Session["Role"].ToString() != "Author")
            {
                TempData["ErrorMessage"] = "You do not have permission to edit blogs.";
                return RedirectToAction("Index", "Blog");
            }

            // Check if this blog belongs to the current author
            int authorId = Convert.ToInt32(Session["AuthorId"]);
            if (blog.author_id != authorId)
            {
                TempData["ErrorMessage"] = "You can only edit your own blogs.";
                return RedirectToAction("Index", "Blog");
            }

            if (ModelState.IsValid)
            {
                // Get the existing blog to preserve any fields we're not updating
                var existingBlog = db.BLOGs.Find(blog.blog_id);
                if (existingBlog == null)
                {
                    return HttpNotFound();
                }

                // Only update title and content
                existingBlog.title = blog.title;
                existingBlog.content = blog.content;

                // Note: We're not updating publication_date

                db.Entry(existingBlog).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Blog has been updated successfully.";
                return RedirectToAction("Index");
            }

            return View(blog);
        }

        // POST: Blog/DeleteAjax/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAjax(int id)
        {
            try
            {
                // Check if user is an author
                if (Session["Role"] == null || Session["Role"].ToString() != "Author")
                {
                    return Json(new { success = false, message = "You do not have permission to delete blogs." });
                }

                BLOG blog = db.BLOGs.Find(id);
                if (blog == null)
                {
                    return Json(new { success = false, message = "Blog not found." });
                }

                // Check if this blog belongs to the current author
                int authorId = Convert.ToInt32(Session["AuthorId"]);
                if (blog.author_id != authorId)
                {
                    return Json(new { success = false, message = "You can only delete your own blogs." });
                }

                // Store blog title for confirmation message
                string blogTitle = blog.title;

                // Delete the blog
                db.BLOGs.Remove(blog);
                db.SaveChanges();

                return Json(new { success = true, message = $"Blog '{blogTitle}' has been deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
