﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Qaud.EntityFramework
{
    public class EFDataStore<T> : IDataStore<T> where T : class
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<T> _dbSet;
        private readonly EntityMemberResolver<T> _memberResolver;
        private bool _autoSave;

        public EFDataStore(DbSet<T> dbSet, DbContext dbContext, bool autoSave = true)
            : this(dbSet)
        {
            _dbContext = dbContext;
            _autoSave = autoSave;
        }

        public EFDataStore(DbSet<T> dbSet)
        {
            _dbSet = dbSet;
            _memberResolver = new EntityMemberResolver<T>();
        }


        public DbSet<T> DataSetImplementation
        {
            get { return _dbSet; }
        }

        /// <summary>
        ///     Returns the underlying <see cref="DbSet{T}" />
        /// </summary>
        public DbContext DataContextImplementation
        {
            get { return _dbContext; }
        }

        object IDataStore<T>.DataSetImplementation
        {
            get { return DataSetImplementation; }
        }

        /// <summary>
        ///     If provided during instantiation, returns the <see cref="DbContext" /> with which the underlying DbSet is
        ///     associated.
        /// </summary>
        object IDataStore<T>.DataContextImplementation
        {
            get { return DataContextImplementation; }
        }

        ///////////////////////////////////////////

        /// <summary>
        ///     Returns an instance of type <typeparamref name="T" />. Note that the returned object must be separately added if
        ///     intended for insertion.
        /// </summary>
        /// <returns></returns>
        public virtual T Create()
        {
            return _dbSet.Create();
        }

        /// <summary>
        ///     Adds the given item to the underlying DbSet.
        /// </summary>
        /// <param name="item"></param>
        public virtual void Add(T item)
        {
            _dbSet.Add(item);
            if (AutoSave) SaveChanges();
        }

        /// <summary>
        ///     Inserts the given item to the underlying DbSet and outputs the resulting entity with any mutations made during
        ///     insertion (i.e. autogenerated identity, etc).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="result"></param>
        public virtual void Add(T item, out T result)
        {
            if (!AutoSave)
            {
                throw new InvalidOperationException(
                    "A DbContext must be provided during initialization and AutoSave must be enabled in order to use this implementation of Add.");
            }
            _dbSet.Add(item);
            _dbContext.SaveChanges();
            result = item;
        }

        /// <summary>
        ///     Adds the given items to the underlying DbSet.
        /// </summary>
        /// <param name="items"></param>
        public virtual void AddRange(IEnumerable<T> items)
        {
            _dbSet.AddRange(items); // EF 6+

            // EF 4.x-5.x
            //foreach (var item in items)
            //{
            //    _dbSet.Add(item);
            //}

            if (AutoSave) SaveChanges();
        }

        public T FindMatch(T lookup)
        {
            var keyprops = _memberResolver.KeyPropertyMembers;
            if (!keyprops.Any()) throw new InvalidOperationException("Type does not have key columns: " + typeof(T).FullName);
            return Find(_memberResolver.GetKeyPropertyValues(lookup));
        }

        public T Find(params object[] keyvalue)
        {
            return _dbSet.Find(keyvalue);
        }

        /// <summary>
        ///     Returns the underlying DbSet as an <see cref="IQueryable{T}" />
        /// </summary>
        public virtual IQueryable<T> Query
        {
            get { return _dbSet; }
        }

        /// <summary>
        ///     Instructs the underlying DbSet to attach the given items, and, if <see cref="AutoSave" />
        ///     is
        ///     <value>true</value>
        ///     , applies the changes to the underlying store.
        /// </summary>
        /// <param name="items"></param>
        public virtual void UpdateRange(IEnumerable<T> items)
        {
            bool asave = _autoSave;
            _autoSave = false;
            foreach (T item in items) Update(item);
            _autoSave = asave;
            if (AutoSave) SaveChanges();
        }

        /// <summary>
        ///     Instructs the underlying DbSet to attach the given item, and, if <see cref="AutoSave" />
        ///     is
        ///     <value>true</value>
        ///     , applies the change to the underlying store.
        /// </summary>
        /// <param name="item"></param>
        public virtual void Update(T item)
        {
            _dbSet.Attach(item);
            if (_dbContext != null)
            {
                _dbContext.Entry(item).State = EntityState.Modified;
            }
            else
            {
                T actual = FindMatch(item);
                _memberResolver.ApplyChanges(actual, item);
            }
            if (AutoSave) SaveChanges();
        }

        /// <summary>
        ///     Removes the given item from the underlying DbSet.
        /// </summary>
        /// <param name="item"></param>
        public virtual void Delete(T item)
        {
            _dbSet.Remove(item);
            if (AutoSave) SaveChanges();
        }

        public void DeleteByKey(params object[] keyvalue)
        {
            Delete(Find(keyvalue));
        }

        public virtual void DeleteRange(IEnumerable<T> items)
        {
            _dbSet.RemoveRange(items); // EF 6

            //// EF 4.x-5.x
            //foreach (var item in items)
            //{
            //    _dbSet.Remove(item);
            //}

            if (AutoSave) SaveChanges();
        }

        public virtual bool AutoSave
        {
            get { return _autoSave; }
            set { _autoSave = value; }
        }

        public virtual void SaveChanges()
        {
            if (_dbContext == null) throw new InvalidOperationException("No DbContext with which to save changes.");
            _dbContext.SaveChanges();
        }

        /// <summary>
        ///     Uses reflection to update a portion of the specified item.
        /// </summary>
        /// <param name="item"></param>
        public virtual void UpdatePartial(object changes)
        {
            T target = _dbSet.Find(_memberResolver.GetKeyPropertyValues(changes).ToArray());
            _dbSet.Attach(target);
            _memberResolver.ApplyPartial(target, changes);
            if (AutoSave) SaveChanges();
        }

        /// <summary>
        /// Gets whether a single property can be deserialized as a complete complex type automatically, whether
        /// via <see cref="SupportsNestedRelationships"/> (navigation properties) or via tree-based document storage.
        /// Returns false if the document store only supports flat table structures, with no relationships.
        /// </summary>
        public virtual bool SupportsNestedRelationships
        {
            get { return true; }
        }

        public bool SupportsComplexStructures
        {
            get { return true; }
        }

        /// <summary>
        /// When implemented, gets whether the data store implementation supports transaction scopes
        /// such as when using <code>using (var transaction = new TransactionScope()) { .. }</code>
        /// </summary>
        public bool SupportsTransactionScope
        {
            get { return true; }
        }
    }
}