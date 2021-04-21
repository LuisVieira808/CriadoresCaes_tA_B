using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CriadoresCaes_tA_B.Data;
using CriadoresCaes_tA_B.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace CriadoresCaes_tA_B.Controllers
{
    public class FotografiasController : Controller
    {

       /// <summary>
       /// este atributo representa a base de dados do projeto
       /// </summary>
        private readonly CriadoresCaesDB _context;

        private readonly IWebHostEnvironment _caminho;

        public FotografiasController(CriadoresCaesDB context, IWebHostEnvironment caminho) {
            _context = context;
            _caminho = caminho;
        }

        // GET: Fotografias
        public async Task<IActionResult> Index()
        {
            //criação de variavel que vai conter um conjunto de dados
            //vindos da base de dados

            var fotografias = _context.Fotografias.Include(f => f.Cao);
            
            
            return View(await fotografias.ToListAsync());
        }

        // GET: Fotografias/Details/5
        /// <summary>
        /// mostra os detalhes de uma fotografia
        /// </summary>
        /// <param name="id">indentificador de fotografia</param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id){
            if (id == null) {
                //entro aqui se nao foi especificado o ID

                //redirecionar para a pagina de inicio
                return RedirectToAction("Index");

                //return NotFound();
            }
            //se chegar aqui, foi especificado um ID
            //vou procurar se existe uma fotografia com esse valor
            var fotografia = await _context.Fotografias
                        .Include(f => f.Cao)
                        .FirstOrDefaultAsync(f => f.Id == id);
            if (fotografia == null) {
                // o ID especificado nao corresponde a uma fotografia
                //return NotFound();
                //redirecionar para a pagina de inicio
                return RedirectToAction("Index");
            }
            //se cheguei aqui é pq a foto existe e foi encontrada
            //mostro-a na view
            return View(fotografia);
        }

        // GET: Fotografias/Create
        /// <summary>
        /// invoca, na primeira vez, a view com os dados de criação de uma fotografia
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            /*geração da lista de valores disponiveis na dropdown
            o view transporta dodos a serem associados ao atributo 'CaoFK'
            o selectlist é um tipo de daods especial que serve para armazenar a lista de opçoes
            de um objecto do tipo <SELECT> do HTML
            Contem dois valores: ID + nome a ser apresnetado no ecra

            _context.Caes: representa a fonte dos dados na 
                           pratica estamos a executar o comando sql
                           Select * From Caes
            vamos alterar a pesquisa para significar 
            Select * From Caes Order By Nome 
             
            
             */

            ViewData["CaoFK"] = new SelectList(_context.Caes.OrderBy(c=>c.Nome), "Id", "Nome");
            return View();
        }

        // POST: Fotografias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DataFoto,Local,CaoFK")] Fotografias foto, IFormFile fotoCao){
            //avaliar se p utilizador escolheu uma opcao valida na dropdown do cao
            if (foto.CaoFK < 0) {
                //não foi escolhido um cao valido
                ModelState.AddModelError("", "Não se esqueça de escolher um cão..");
            }
            /*processar ficheiro
                -existe?
                    -se não existe, o que fazer? => gerar uma mensagem de erro e devolver controlo a view
                    -se continuo é porque o ficheiro existe
                        -mas sera que é do tipo correto?
                            -avaliar se é imagem
                                -se sim:-especificar o seu novo nome 
                                        -especificar a localização
                                        -associar ao objeto foto o nome deste ficheiro
                                        -guardar ficheiro no disco rigido do servidor
                                -se não: => gerar uma mensagem de erro e devolver controlo a view
             */
            //defenir o novo nome da fotografia
            string nomeImagem = "";
            if (fotoCao == null) {
                //não existe ficheiro
                //adicionar msm de erro
                ModelState.AddModelError("", "Adicione, por favor, a fotografia do cão");
                //devolver controlo a view
                ViewData["CaoFK"] = new SelectList(_context.Caes.OrderBy(c => c.Nome), "Id", "Nome");
                return View(foto);
            } else {
                //ha ficheiro. mas será um ficheiro valido?
                if(fotoCao.ContentType=="image/jpeg"|| fotoCao.ContentType == "image/png") {
                    
                    Guid g;
                    g = Guid.NewGuid();
                    nomeImagem = foto.CaoFK + "_" + g.ToString();
                    //determinar a extenção da imagem
                    string extensao = Path.GetExtension(fotoCao.FileName).ToLower();
                    //agora consigo ter o nome final do ficheiro
                    nomeImagem = nomeImagem + extensao;

                    //associar este ficheiro aos dados da fotografia do cao
                    foto.Fotografia = nomeImagem;

                    //localização do armazenamento da imagem
                    string localizacaoFicheiro = _caminho.WebRootPath;
                    nomeImagem = Path.Combine(localizacaoFicheiro,"fotos",nomeImagem);



                } else {
                    ModelState.AddModelError("", "Adicione, por favor, a fotografia do cão");
                    //devolver controlo a view
                    ViewData["CaoFK"] = new SelectList(_context.Caes.OrderBy(c => c.Nome), "Id", "Nome");
                    return View(foto);
                }
            }

            

                if (ModelState.IsValid) {
                    try {
                        //adicionar os dados da nova fotografia a base de dados
                        _context.Add(foto);
                        //consolidar os dados na base de dados
                        await _context.SaveChangesAsync();

                        //se cheguei ate aqui tudo correu bem 
                       //vou guardar agora no disco rigido do servidor a imagem
                        using var stream = new FileStream(nomeImagem, FileMode.Create);
                        await fotoCao.CopyToAsync(stream);
                        
                        
                        return RedirectToAction(nameof(Index));
                    
                     } catch (Exception) {
                        ModelState.AddModelError("", "Ocorreu um erro...");
                    }
                }                
            
            else {
                
            }
            ViewData["CaoFK"] = new SelectList(_context.Caes.OrderBy(c => c.Nome), "Id", "Nome", foto.CaoFK);
            return View(foto);
        }

        // GET: Fotografias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fotografias = await _context.Fotografias.FindAsync(id);
            if (fotografias == null)
            {
                return NotFound();
            }
            ViewData["CaoFK"] = new SelectList(_context.Caes, "Id", "Id", fotografias.CaoFK);
            return View(fotografias);
        }

        // POST: Fotografias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fotografia,DataFoto,Local,CaoFK")] Fotografias fotografias)
        {
            if (id != fotografias.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fotografias);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FotografiasExists(fotografias.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CaoFK"] = new SelectList(_context.Caes, "Id", "Id", fotografias.CaoFK);
            return View(fotografias);
        }

        // GET: Fotografias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fotografias = await _context.Fotografias
                .Include(f => f.Cao)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fotografias == null)
            {
                return NotFound();
            }

            return View(fotografias);
        }

        // POST: Fotografias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fotografias = await _context.Fotografias.FindAsync(id);
            try {
                //proteger a eleminação de uma foto
                _context.Fotografias.Remove(fotografias);
                await _context.SaveChangesAsync();

                //nao esquecer remover o ficheiro da fotografia do disco rigido


            } catch (Exception) {

                throw;
            }
            
            
            return RedirectToAction(nameof(Index));
        }

        private bool FotografiasExists(int id)
        {
            return _context.Fotografias.Any(e => e.Id == id);
        }
    }
}
