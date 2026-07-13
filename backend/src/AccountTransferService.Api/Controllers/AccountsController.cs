using AccountTransferService.Api.Dtos;
using AccountTransferService.Domain.Exceptions;
using AccountTransferService.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccountTransferService.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetAccounts(CancellationToken cancellationToken)
    {
        var accounts = await _context.Accounts
            .OrderBy(a => a.AccountNumber)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                OwnerName = a.OwnerName,
                Balance = a.Balance,
                Currency = a.Currency
            })
            .ToListAsync(cancellationToken);

        return Ok(accounts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountDto>> GetAccount(Guid id, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .Where(a => a.Id == id)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                OwnerName = a.OwnerName,
                Balance = a.Balance,
                Currency = a.Currency
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (account is null)
        {
            throw new AccountNotFoundException(id);
        }

        return Ok(account);
    }
}
