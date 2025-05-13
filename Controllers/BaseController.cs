using Microsoft.AspNetCore.Mvc;
using BestReads.Repositories;

namespace BestReads.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController<T> : ControllerBase where T : class {
    protected readonly BaseRepository<T> _repository;

    public BaseController(BaseRepository<T> repository) {
        _repository = repository;
    }

    [HttpGet]
    public abstract Task<ActionResult<IEnumerable<T>>> GetAll();

    [HttpGet("{id}")]
    public abstract Task<ActionResult<T>> GetById(string id);

    [HttpPost]
    public abstract Task<ActionResult<T>> Create(T entity);

    [HttpPut("{id}")]
    public abstract Task<ActionResult<T>> Update(string id, T entity);

    [HttpDelete("{id}")]
    public abstract Task<ActionResult<T>> Delete(string id);
}
