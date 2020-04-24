using ProjectDomain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDomain.Business.Login
{
    public interface ILoginBusiness
    {
        List<AccountDTO> findAllAccount();
        AccountDTO findById(string id);
        void createAccount(AccountDTO newAccount);
        void updateAccount(AccountDTO currentAccount);
    }
}
