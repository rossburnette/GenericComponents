﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Commons.Model;
using Commons.Repository;
using Commons.RequestModel;
using Commons.ViewModel;

namespace Commons.Service
{
    public class BaseService<T, TRm, TVm> : IBaseService<T> where T : Entity where TRm : RequestModel<T> where TVm : BaseViewModel<T>
    {
        protected BaseRepository<T> Repository;

        public BaseService(BaseRepository<T> repository)
        {
            Repository = repository;
        }

        public virtual bool Add(T entity)
        {
            var add = Repository.Add(entity);
            var save = Repository.Save();
            return save;
        }
         

        public bool Delete(T entity)
        {
            bool deleted = Repository.Delete(entity);
            Repository.Save();
            return deleted;
        }

        public bool Delete(string id)
        {
            var entity = Repository.Filter(x => x.Id == id).FirstOrDefault();
            bool deleted = Repository.Delete(entity);
            Repository.Save();
            return deleted;
        }

        public virtual bool Edit(T entity)
        {
            bool edit = Repository.Edit(entity);
            Repository.Save();
            return edit;
        }

        public T GetById(string id)
        {
            return Repository.GetById(id);
        }         
         
        public async Task<List<TVm>> GetAllAsync()
        {
            var queryable = await Repository.Get().ToListAsync();
            var vms = queryable.Select(x => (TVm) Activator.CreateInstance(typeof(TVm), new object[] {x}));
            return  vms.ToList();
        }
       
        public List<DropdownViewModel> GetDropdownListAsync(TRm request)
        {
            IQueryable<T> queryable = Repository.Get();
            queryable = request.GetOrderedData(queryable);
            List<DropdownViewModel> list = queryable.Select(request.Dropdown()).ToList();
            return list;
        }
        
        public async Task<Tuple<List<TVm>,int>> SearchAsync(TRm request)
        {
            var queryable = request.GetOrderedData(Repository.Get());
            int count = queryable.Count();
            queryable = request.SkipAndTake(queryable);
            queryable = request.IncludeParents(queryable);
            var list = await queryable.ToListAsync();
            List<TVm> vms = list.ConvertAll(CreateVmInstance);
            return new Tuple<List<TVm>, int>(vms,count);
        }

        private static TVm CreateVmInstance(T x)
        {
            return (TVm)Activator.CreateInstance(typeof(TVm), x);
        }

        public  TVm GetDetail(string id)
        {
            var model = Repository.GetById(id);
            if (model == null)
            {
                return null;
            }
            return CreateVmInstance(model);
        }        
    }
}
