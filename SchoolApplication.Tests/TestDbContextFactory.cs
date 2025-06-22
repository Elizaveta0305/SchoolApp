using Microsoft.EntityFrameworkCore;
using SchoolApplication.Data;
using SchoolApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolApplication.Tests
{
    public class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly string _databaseName;
        // Используем статический Dictionary для отслеживания инициализации баз данных
        // по их уникальному имени, чтобы EnsureDeleted/EnsureCreated вызывались только один раз для КАЖДОЙ уникальной базы.
        private static readonly Dictionary<string, bool> _databaseInitializedFlags = new Dictionary<string, bool>();

        public TestDbContextFactory()
        {
            _databaseName = Guid.NewGuid().ToString(); // Уникальное имя для каждой инстанции фабрики
        }

        public ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options;

            var context = new ApplicationDbContext(options);

            // Синхронизированный доступ к флагу инициализации
            lock (_databaseInitializedFlags)
            {
                if (!_databaseInitializedFlags.ContainsKey(_databaseName) || !_databaseInitializedFlags[_databaseName])
                {
                    context.Database.EnsureDeleted(); // Удаляем базу данных, если она существует
                    context.Database.EnsureCreated(); // Создаем новую базу данных
                    _databaseInitializedFlags[_databaseName] = true; // Устанавливаем флаг
                }
            }
            context.ChangeTracker.Clear(); // Очищаем ChangeTracker после создания/получения контекста
            return context;
        }

        public void SeedData(ApplicationDbContext context, params object[] entities)
        {
            // Очищаем ChangeTracker перед засеиванием, чтобы избежать конфликтов
            context.ChangeTracker.Clear();

            // Добавляем сущности в правильном порядке, чтобы EF Core мог правильно отслеживать связи
            foreach (var entity in entities)
            {
                // Используем Add() вместо AddRange() для более точного контроля и отслеживания связанных сущностей
                // EF Core должен автоматически устанавливать состояние Added для связанных сущностей,
                // если они еще не отслеживаются.
                context.Add(entity);
            }
            context.SaveChanges();
            context.ChangeTracker.Clear(); // Очищаем ChangeTracker после сохранения
        }
    }
}