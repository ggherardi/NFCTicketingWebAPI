using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NFCTicketing;

namespace NFCTicketingWebAPI
{
    public class Utility
    {
        public static EncryptableSmartTicket ConvertToEncryptableSmartTicket(SmartTicket ticket)
        {
            string[] cardIdStringArray = ticket.CardId.Split("-");
            byte[] cardId = new byte[cardIdStringArray.Length];
            for(int i = 0; i < cardIdStringArray.Length; i++)
            {
                cardId[i] = Convert.ToByte(cardIdStringArray[i], 16);
            }
            EncryptableSmartTicket encryptableTicket = new EncryptableSmartTicket()
            {
                CardID = cardId,
                Credit = ticket.Credit,
                CurrentValidation = ticket.CurrentValidation,
                SessionValidation = ticket.SessionValidation,
                UsageTimestamp = ticket.UsageTimestamp,
                SessionExpense = ticket.SessionExpense,
                TicketTypeName = ticket.TicketType                
            };
            return encryptableTicket;
        }
    }
}
