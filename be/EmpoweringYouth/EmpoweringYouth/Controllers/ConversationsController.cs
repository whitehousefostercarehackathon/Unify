﻿using EmpoweringYouth.Context;
using EmpoweringYouth.Models;
using EmpoweringYouth.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace EmpoweringYouth.Controllers
{
    [RoutePrefix("api/conversations")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ConversationsController : ApiController
    {

        [HttpGet]
        [Route("{conversationId:int}")]
        public ConversationData GetConversation(int conversationId)
        {

            var requestUser = ControllerUtils.GetUserFromRequest(Request);
            using (var ctx = new EmpoweringYouthContext())
            {
                var persistenUser = ctx.users.Find(requestUser.Id);
                var conversation = ctx.conversations.Include("To").Include("From").Include("Messages").Include("Messages.From").Include("Messages.To").
                    Where(conv => conv.Id == conversationId && (conv.From.Id == persistenUser.Id || conv.To.Id == persistenUser.Id)).First();
                ConversationData c = new ConversationData();
                c.From = conversation.From;
                c.To = conversation.To;
                c.StartedDate = conversation.StartedDate;
                c.ReplyIn = conversation.ReplyIn;
                c.Subject = conversation.Subject;
                c.messages = conversation.Messages.OrderByDescending(m => m.Date).ToList();
                return c;
            }
        }

        [HttpGet]
        public List<ConversationData> GetConversations()
        {
            var requestUser = ControllerUtils.GetUserFromRequest(Request);
            using (var ctx = new EmpoweringYouthContext())
            {
                var persistenUser = ctx.users.Find(requestUser.Id);
                var conversations = ctx.conversations.Include("To").Include("From").ToList();
                conversations = conversations.Where(c => (c.To.Id == requestUser.Id || c.From.Id == requestUser.Id)).ToList();
                var conversationsData = new List<ConversationData>();
                foreach (Conversation c in conversations)
                {
                    conversationsData.Add(new ConversationData
                    {
                        Id = c.Id,
                        From = c.From,
                        To = c.To,
                        LastMessageDate = c.LastMessageDate,
                        Subject = c.Subject,
                        StartedDate = c.StartedDate,
                        messages = c.Messages.OrderByDescending(m => m.Date).ToList()

                    });
                }
                return conversationsData;
            }
        }

        [Route("summary")]
        public IDictionary<String, long> GetConversationsSummary()
        {
            var requestUser = ControllerUtils.GetUserFromRequest(Request);
            using (var ctx = new EmpoweringYouthContext())
            {
                var persistenUser = ctx.users.Find(requestUser.Id);
                var conversations = ctx.conversations.Include("To").Include("From").Include("Messages").Include("Messages.From").Include("Messages.To").
                    Where(conv => (conv.From.Id == requestUser.Id || conv.To.Id == requestUser.Id)).ToList();

                var result = new Dictionary<String, long>();
                var unreadMessages = conversations.Where(c => c.Messages.OrderByDescending(m => m.Date).First().Status == Status.UNREAD).Count();
                result.Add("unread", unreadMessages);
                return result;
            }

        }

        [Route("dummy")]
        public long GetConversationsCount()
        {
            using (var ctx = new EmpoweringYouthContext())
            {
                return ctx.conversations.Count();
            }
        }
    }
}