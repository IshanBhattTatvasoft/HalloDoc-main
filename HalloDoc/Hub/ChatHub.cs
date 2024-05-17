using DocumentFormat.OpenXml.Spreadsheet;
using HalloDoc.DataLayer.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using HalloDoc.LogicLayer.Patient_Interface;

namespace HalloDoc.DataLayer.ViewModels;
public class ChatHub : Hub
{
    private readonly IJwtToken _jwt;
    private readonly IAdminInterface _admin;
    public ChatHub(IJwtToken jwt, IAdminInterface admin)
    {
        _jwt = jwt;
        _admin = admin;
    }

    public override Task OnConnectedAsync()
    {
        HttpContext httpContext = Context.GetHttpContext();
        string jwtToken = httpContext.Request.Cookies["token"];
        string id = _jwt.GetAspId(jwtToken);
        Groups.AddToGroupAsync(Context.ConnectionId, id);
        return base.OnConnectedAsync();
    }

    public async Task SendMessage(string receiverId, string requestId, string user, string message, string receiverRoleName)
    {
        HttpContext httpContext = Context.GetHttpContext();
        string jwtToken = httpContext.Request.Cookies["token"];
        string id = _jwt.GetAspId(jwtToken);
        string senderId = id;

        if(receiverRoleName == "Admin")
        {
            List<string> allAdminIds = _admin.GetAllAdminIds();
            await Clients.Groups(allAdminIds).SendAsync("ReceiveMessage", senderId, requestId, user, message);

        }
        else
        {
            await Clients.Group(receiverId).SendAsync("ReceiveMessage", receiverId, requestId, user, message);
        }
        //await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}

