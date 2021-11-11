using System;
using System.Text.RegularExpressions;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firName, string surname, string email, DateTime dateOfBirth, int clientId)
        {
            // для проверки емейла воспольуземся шаблоном из интернета
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";
            var dateBirth = DateTime.Now.AddYears(-21); //  сразу посчитаем дату, после которой люди слишком молоды для регистраици в этом замечательном проекте
            int isImportant = 1; // использем для отделения знати от работяг
            
            if (string.IsNullOrEmpty(firName) || 
                string.IsNullOrEmpty(surname) ||
                !Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase) ||
                dateOfBirth < dateBirth)
            {
                return false;
            }

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firName,
                Surname = surname
            };

            // для элиты пропустим проверку лимита
            if (client.Name == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else
            {
                if (client.Name == "ImportantClient")
                    isImportant = 2;
             
                user.HasCreditLimit = true;
                using (var userCreditService = new UserCreditServiceClient())
                {
                    var creditLimit = userCreditService.GetCreditLimit(user.FirstName, user.Surname, user.DateOfBirth) * isImportant;
                    user.CreditLimit = creditLimit;
                }
                if (user.CreditLimit < 500)
                {
                    return false;
                }                
            }

            UserDataAccess.AddUser(user);

            return true;
        }
    }
}
