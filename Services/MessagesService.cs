﻿namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using Src.Data;
    using Src.Model;
    using Src.Repos;
    using StockApp.Repositories;

    public class MessagesService : IMessagesService
    {
        private readonly MessagesRepository messagesRepository;

        public MessagesService(MessagesRepository messagesRepository)
        {
            this.messagesRepository = messagesRepository;
        }

        public void GiveMessageToUser(string userCNP)
        {
            DatabaseConnection dbConn = new DatabaseConnection();
            UserRepository userRepository = new UserRepository(dbConn);

            int userCreditScore = userRepository.GetUserByCnp(userCNP).CreditScore;
            try
            {
                if (userCreditScore >= 550)
                {
                    this.messagesRepository.GiveUserRandomMessage(userCNP);
                }
                else
                {
                    this.messagesRepository.GiveUserRandomRoastMessage(userCNP);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message},User is not found");
            }
        }

        public List<Message> GetMessagesForGivenUser(string userCnp)
        {
            return this.messagesRepository.GetMessagesForGivenUser(userCnp);
        }
    }
}
