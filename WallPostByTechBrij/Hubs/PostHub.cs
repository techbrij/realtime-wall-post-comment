using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.SignalR;
using WallPostByTechBrij.Models;
using WebMatrix.WebData;

namespace WallPostByTechBrij.Hubs
{    
    public class PostHub : Hub
    {
        private string imgFolder = "/Images/profileimages/";
        private string defaultAvatar = "user.png";

        // GET api/WallPost
        public void GetPosts()
        {
            using (WallEntities db = new WallEntities())
            {
                var ret = (from post in db.Posts.ToList()
                           orderby post.PostedDate descending
                           select new
                           {
                               Message = post.Message,
                               PostedBy = post.PostedBy,
                               PostedByName = post.UserProfile.UserName,
                               PostedByAvatar = imgFolder + (String.IsNullOrEmpty(post.UserProfile.AvatarExt) ? defaultAvatar : post.PostedBy + "." + post.UserProfile.AvatarExt),
                               PostedDate = post.PostedDate,
                               PostId = post.PostId,
                               PostComments = from comment in post.PostComments.ToList()
                                              orderby comment.CommentedDate
                                              select new
                                              {
                                                  CommentedBy = comment.CommentedBy,
                                                  CommentedByName = comment.UserProfile.UserName,
                                                  CommentedByAvatar = imgFolder + (String.IsNullOrEmpty(comment.UserProfile.AvatarExt) ? defaultAvatar : comment.CommentedBy + "." + comment.UserProfile.AvatarExt),
                                                  CommentedDate = comment.CommentedDate,
                                                  CommentId = comment.CommentId,
                                                  Message = comment.Message,
                                                  PostId = comment.PostId

                                              }
                           }).ToArray();
                Clients.All.loadPosts(ret);
            }
        }

        public void AddPost(Post post)
        {
            post.PostedBy = WebSecurity.CurrentUserId;
            post.PostedDate = DateTime.UtcNow;
            using (WallEntities db = new WallEntities())
            {
                db.Posts.Add(post);
                db.SaveChanges();
                var usr = db.UserProfiles.FirstOrDefault(x => x.UserId == post.PostedBy);
                var ret = new
                {
                    Message = post.Message,
                    PostedBy = post.PostedBy,
                    PostedByName = usr.UserName,
                    PostedByAvatar = imgFolder + (String.IsNullOrEmpty(usr.AvatarExt) ? defaultAvatar : post.PostedBy + "." + post.UserProfile.AvatarExt),
                    PostedDate = post.PostedDate,
                    PostId = post.PostId
                };

                Clients.Caller.addPost(ret);
                Clients.Others.newPost(ret);
            }
        }

        public dynamic AddComment(PostComment postcomment)
        {
            postcomment.CommentedBy = WebSecurity.CurrentUserId;
            postcomment.CommentedDate = DateTime.UtcNow;
            using (WallEntities db = new WallEntities())
            {
                db.PostComments.Add(postcomment);
                db.SaveChanges();
                var usr = db.UserProfiles.FirstOrDefault(x => x.UserId == postcomment.CommentedBy);
                var ret = new
                {
                    CommentedBy = postcomment.CommentedBy,
                    CommentedByName = usr.UserName,
                    CommentedByAvatar = imgFolder + (String.IsNullOrEmpty(usr.AvatarExt) ? defaultAvatar : postcomment.CommentedBy + "." + postcomment.UserProfile.AvatarExt),
                    CommentedDate = postcomment.CommentedDate,
                    CommentId = postcomment.CommentId,
                    Message = postcomment.Message,
                    PostId = postcomment.PostId
                };
                Clients.Others.newComment(ret, postcomment.PostId);
                return ret;
            }
        }

             
    }
}