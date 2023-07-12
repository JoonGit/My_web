using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlog.Data;
using MyBlog.Data.Service;
using MyBlog.Models;

namespace MyBlog.Controllers
{
    [Route("Product")]
    public class ProductController : Controller
    {

        private readonly IProudctesService _service;
        private readonly UserManager<NewIdentityUser> _userManager;
        private readonly BlogDbContext _dbContext;


        public ProductController(
            IProudctesService service
            , UserManager<NewIdentityUser> userManager
            , BlogDbContext dbContext

            )
        {
            _service = service;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        #region 상품등록
        [HttpGet("register")]
        public async Task<IActionResult> Register()
        {
            // 사용자 정보는 나중에 캐시나 세션으로 값 받아오는 걸로 해결
            ViewBag.userId =_userManager.GetUserId(HttpContext.User);
            return View();
        }

        // 권한을 소비자 등록자 로 나워 등록자만 접근 가능하도록 변경
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(ProductModel model, IFormFileCollection files)
        {
            // 값 hidden
            //if (!ModelState.IsValid) return View(model);
            //model.RegisterUserId = _userManager.GetUserId(HttpContext.User);
            // hidden으로 받은 값으로 경로 설정

            if(await _service.Upload(model, files) > 0)
            {
                return View(model);
            }
            // 파일 업로드후 모델 저장
            await _service.AddAsync(model);
            return Redirect("/product/list");  
        }
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        #endregion


        #region 상품목록조회
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            // 전체 상품 목록 조회
            var result = _dbContext.Products.ToList();
            return View(result);
        }
        #endregion

        #region 상품상세조회

        [HttpGet("read/{id:int}")]
        public async Task<IActionResult> Read(int id)
        {
            // 상품의 Id로 해당 물건 정보 가져오기
            var result = _service.GetByIdAsync(id);
            return View(result);
        }
        #endregion

        #region 상품수정
        [HttpGet("Edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            // 수정할 상품 정보 불러오기
            var result = _service.GetByIdAsync(id);
            return View(result.Result);
        }
        [HttpPost("Edit")]
        public IActionResult Edit(ProductModel model)
        {
            // 변경사항 없으면 변경 X
            var UpdateModel = _dbContext
                            .Products
                            .Where(product => product.Id == model.Id)
                            .FirstOrDefault();

            UpdateModel.Name = model.Name;
            UpdateModel.Price = model.Price;

            if(UpdateModel.URI != model.URI)
            {
                Delete(model.Id);
                //RegisterAsync()
            }

            _dbContext.SaveChanges();
            // 필요한 부분만 변경을 원하면
            //_service.UpdateAsync(Id, model);
            return Redirect("/product/list");
        }
        #endregion

        #region 상품삭제
        // 상품 삭제
        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            // 상품 삭제시 파일도 삭제
            var result = _service.GetByIdAsync(id);
            // 파일 삭제
            string path = @"wwwroot/" + result.Result.URI;
            if (System.IO.File.Exists(path))
            {
                try
                {
                    System.IO.File.Delete(path);
                }
                catch (System.IO.IOException e)
                {
                    await Console.Out.WriteLineAsync(e.Message);
                    // handle exception
                }
            }

            // 상품 삭제
            await _service.DeleteAsync(id);
            
            return Redirect("/product/list");
        }
        #endregion

        #region 장바구니
        [HttpGet("buy/{id:int}")]
        public async Task<IActionResult> buy(int id)
        {
            var user = HttpContext.User;
            // 현재 유저의 구매 이력 만들기 
            var curUser = await _userManager.GetUserAsync(user);
            BuyListModel buyList = new BuyListModel()
            {
                // 현재 구매하는 상품의 id
                ProductModelId = id,
                // 구매하는 유저의 id
                NewIdentityUserId = curUser.Id,
            };
            _dbContext.Add(buyList);
            _dbContext.SaveChanges();
            return Redirect("/product/list");
        }
        #endregion

        #region 상품 찜 하기
        // 상품 찜 하기
        [HttpGet("Wish/{id:int}")]
        public async Task<string> Wish(int id)
        {
            var user = HttpContext.User;
            // 상품의 id와 유저의 정보를 확인해 찜하기, 정보가 있으면 취소

            var curUser = await _userManager.GetUserAsync(user);

            // 찜을 누른 상품과 좋아요를 누른 사람의 목록 가져오기
            var result = _dbContext.Products.Include(p => p.WishUsers).Where(p => p.Id == id).FirstOrDefault();

            if (result.WishUsers.Count == 0)
            {
                result.WishUsers.Add(curUser);
                _dbContext.SaveChanges();
                return "ok";
            }
            else
            {
                result.WishUsers.Remove(curUser);
                _dbContext.SaveChanges();
                return "remove";
            }
        }
        #endregion
    }
}
