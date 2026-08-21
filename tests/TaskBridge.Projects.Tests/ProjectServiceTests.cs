using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using TaskBridge.Projects.Services;
using TaskBridge.Projects.Data;
using TaskBridge.Projects.Models.Dtos;
using TaskBridge.Core.Entities;
using TaskBridge.Core.Exceptions;

namespace TaskBridge.Projects.Tests
{
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _repoMock = new();
        private readonly Mock<ILogger<ProjectService>> _loggerMock = new();
        private readonly ProjectService _service;

        public ProjectServiceTests()
        {
            _service = new ProjectService(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateProjectAsync_Should_CreateProject_When_Valid()
        {
            // Arrange
            var dto = new CreateProjectDto { Name = "Test Project", TeamId = "team-1", Description = "desc" };
            var organizationId = "org-1";
            var userId = "user-1";

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Project>(), organizationId))
                .ReturnsAsync((Project p, string org) => p);

            // Act
            var result = await _service.CreateProjectAsync(dto, organizationId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.TeamId, result.TeamId);
            Assert.Equal("PLANNING", result.MilestoneStatus);
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Project>(), organizationId), Times.Once);
        }

        [Fact]
        public async Task CreateProjectAsync_Should_ThrowValidationException_When_DtoNull()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateProjectAsync(null!, "org", "user"));
        }

        [Fact]
        public async Task UpdateMilestoneStatusAsync_Should_UpdateStatus_When_ProjectExists()
        {
            // Arrange
            var projectId = "proj-1";
            var organizationId = "org-1";
            var userId = "user-1";
            var existing = new Project { Id = projectId, OrganizationId = organizationId, MilestoneStatus = "PLANNING" };

            _repoMock.Setup(r => r.GetByIdAsync(projectId, organizationId)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Project>(), organizationId))
                .ReturnsAsync((Project p, string org) => p);

            // Act
            var result = await _service.UpdateMilestoneStatusAsync(projectId, "IN_PROGRESS", organizationId, userId);

            // Assert
            Assert.Equal("IN_PROGRESS", result.MilestoneStatus);
            _repoMock.Verify(r => r.GetByIdAsync(projectId, organizationId), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Project>(), organizationId), Times.Once);
        }

        [Fact]
        public async Task UpdateMilestoneStatusAsync_Should_ThrowNotFound_When_ProjectMissing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((Project?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateMilestoneStatusAsync("missing", "DONE", "org", "user"));
        }

        [Fact]
        public async Task GetProjectsByTeamAsync_Should_Return_List()
        {
            var teamId = "team-1";
            var organizationId = "org-1";
            var projects = new List<Project>
            {
                new Project { Id = "p1", TeamId = teamId, OrganizationId = organizationId, Name = "P1", CreatedAt = DateTime.UtcNow }
            };

            _repoMock.Setup(r => r.GetByTeamAsync(teamId, organizationId)).ReturnsAsync(projects);

            var result = await _service.GetProjectsByTeamAsync(teamId, organizationId);

            Assert.Single(result);
            Assert.Equal("P1", result[0].Name);
        }

        [Fact]
        public async Task GetProjectAsync_Should_ThrowNotFound_When_Missing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((Project?)null);
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetProjectAsync("id", "org"));
        }

        [Fact]
        public async Task DeleteProjectAsync_Should_Delete_When_ProjectExists()
        {
            var projectId = "p1";
            var organizationId = "org";
            var userId = "user";

            var existing = new Project { Id = projectId, OrganizationId = organizationId };
            _repoMock.Setup(r => r.GetByIdAsync(projectId, organizationId)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.DeleteAsync(projectId, organizationId)).Returns(Task.CompletedTask);

            await _service.DeleteProjectAsync(projectId, organizationId, userId);

            _repoMock.Verify(r => r.DeleteAsync(projectId, organizationId), Times.Once);
        }

        [Fact]
        public async Task DeleteProjectAsync_Should_ThrowNotFound_When_Missing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((Project?)null);
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteProjectAsync("id", "org", "user"));
        }
    }
}
