using AutoMapper;
using backend_disc.Dtos.Employees;
using backend_disc.Models;
using backend_disc.Repositories;
using backend_disc.Repositories.StoredProcedureParams;
using class_library_disc.Models.Sql;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace backend_disc.Services.Tests
{
    [TestClass()]
    public class EmployeeServiceTests
    {
        private Mock<IEmployeesRepository> _mockEmployeeRepository = null!;
        private Mock<IUserRepository> _mockUserRepository = null!;
        private Mock<IGenericRepository<Company>> _mockCompanyRepository = null!;
        private Mock<IMapper> _mockMapper = null!;
        private EmployeeService _employeeService = null!;
        private Company _company = null!;
        private const int TEST_ITERATIONS = 2;

        private const string DEFAULT_IMAGE_PATH = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png";
        private const string PASSWORD_HASH = "$argon2id$v=19$m=65536,t=3,p=1$JcD7uPdQ3ey8lapNPowUmg$ulD90DajUEOpnbsnmY1Q/pkNeoLArY5XXJlpbRi4QcY";
        private const string VALID_CPR = "12345678";
        private const int VALID_DEPARTMENT_ID = 1;
        private const int VALID_DISC_PROFILE_ID = 1;
        private const string VALID_FIRST_NAME = "John";
        private const string VALID_LAST_NAME = "Doe";
        private const int VALID_POSITION_ID = 1;
        private const string VALID_USERNAME = "johndo123";
        private const string VALID_WORK_EMAIL = VALID_USERNAME + "@test.com";
        private const string VALID_WORK_PHONE = "87654321";
        private const string VALID_PRIVATE_EMAIL = "private@test.com";
        private const string VALID_PRIVATE_PHONE = "12345678";
        private const int VALID_USER_ROLE_ID = 1;

        private CreateNewEmployee _validDtoEmployee = null!;
        private AddEmployeeSpParams _validSpParamsEmployee = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockEmployeeRepository = new Mock<IEmployeesRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockCompanyRepository = new Mock<IGenericRepository<Company>>();
            _mockMapper = new Mock<IMapper>();
            _company = new Company { Id = 1, Name = "TechCorp", BusinessField = "Software", Location = "Copenhagen" };
            _mockCompanyRepository.Setup(x => x.GetById(1)).ReturnsAsync(_company);

            var employees = new List<Employee>
            {
                new Employee {Id = 1, WorkEmail = "admin@techcorp.com", WorkPhone = "88888927", FirstName = "Admin", LastName = "Admin", ImagePath = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png", DepartmentId = 1, DiscProfileId = 1, PositionId = 1},
                new Employee {Id = 2, WorkEmail = "alice@techcorp.com", WorkPhone = "88887777", FirstName = "Alice", LastName = "Jensen", ImagePath = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png", DepartmentId = 1, DiscProfileId = 1, PositionId = 1},
                new Employee {Id = 7, WorkEmail = "noah@techcorp.com", WorkPhone = "88887890", FirstName = "Noah", LastName = "Larsen", ImagePath = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png", DepartmentId = 1, DiscProfileId = 1, PositionId = 1},
            };

            _mockUserRepository.Setup(x => x.UsernameExists(It.IsAny<string>())).ReturnsAsync(false);
            _mockEmployeeRepository.Setup(x => x.PhoneNumExists(It.IsAny<string>())).ReturnsAsync(false);
            var paginatedList = new PaginatedList<Employee>(employees, 1, employees.Count, 10);
            _mockEmployeeRepository.Setup(x => x.GetAll(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(paginatedList);
            _employeeService = new EmployeeService(_mockUserRepository.Object, _mockCompanyRepository.Object, _mockMapper.Object, NullLogger<EmployeeService>.Instance, _mockEmployeeRepository.Object);

            _validDtoEmployee = new CreateNewEmployee()
            {
                CPR = VALID_CPR,
                DepartmentId = VALID_DEPARTMENT_ID,
                DiscProfileId = VALID_DISC_PROFILE_ID,
                FirstName = VALID_FIRST_NAME,
                LastName = VALID_LAST_NAME,
                PositionId = VALID_POSITION_ID,
                PrivateEmail = VALID_PRIVATE_EMAIL,
                PrivatePhone = VALID_PRIVATE_PHONE
            };
            _validSpParamsEmployee = new AddEmployeeSpParams()
            {
                CPR = VALID_CPR,
                DepartmentId = VALID_DEPARTMENT_ID,
                DiscProfileId = VALID_DISC_PROFILE_ID,
                FirstName = VALID_FIRST_NAME,
                LastName = VALID_LAST_NAME,
                PositionId = VALID_POSITION_ID,
                PrivateEmail = VALID_PRIVATE_EMAIL,
                PrivatePhone = VALID_PRIVATE_PHONE,
                WorkEmail = VALID_WORK_EMAIL,
                WorkPhone = VALID_WORK_PHONE,
                Username = VALID_USERNAME,
                ImagePath = DEFAULT_IMAGE_PATH,
                PasswordHash = PASSWORD_HASH,
                UserRoleId = VALID_USER_ROLE_ID
            };
        }

        private void SetupCreateEmployeeMocks()
        {
            _mockMapper.Setup(x => x.Map<AddEmployeeSpParams?>(It.IsAny<CreateNewEmployee>())).Returns(_validSpParamsEmployee);
            
            var createdEmployee = new Employee
            {
                Id = 10,
                FirstName = VALID_FIRST_NAME,
                LastName = VALID_LAST_NAME,
                WorkEmail = VALID_WORK_EMAIL,
                WorkPhone = VALID_WORK_PHONE,
                ImagePath = DEFAULT_IMAGE_PATH,
                DepartmentId = VALID_DEPARTMENT_ID,
                DiscProfileId = VALID_DISC_PROFILE_ID,
                PositionId = VALID_POSITION_ID
            };
            _mockEmployeeRepository.Setup(x => x.AddEmployeeSPAsync(It.IsAny<AddEmployeeSpParams>())).ReturnsAsync(createdEmployee);
            
            var employeeDto = new EmployeeDto
            {
                Id = 10,
                FirstName = VALID_FIRST_NAME,
                LastName = VALID_LAST_NAME,
                WorkEmail = VALID_WORK_EMAIL,
                WorkPhone = VALID_WORK_PHONE,
                ImagePath = DEFAULT_IMAGE_PATH
            };
            _mockMapper.Setup(x => x.Map<EmployeeDto?>(It.IsAny<Employee>())).Returns(employeeDto);
        }

        [TestMethod]
        public async Task CreateEmployee_GeneratesWorkEmailAndUsername_WithCorrectFormat()
        {
            SetupCreateEmployeeMocks();

            var expectedEmailRegex = new System.Text.RegularExpressions.Regex(@"^[a-z]+[a-z]{2}\d{3}@test\.com$");
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                var result = await _employeeService.CreateEmployee(_validDtoEmployee);

                Assert.IsNotNull(result);
                string workEmail = result.WorkEmail;
                int at = workEmail.IndexOf('@');
                string username = at >= 0 ? workEmail.AsSpan(0, at).ToString() : string.Empty;
                Console.WriteLine(workEmail);
                Console.WriteLine(username);
                Assert.IsTrue(workEmail.Length > 5);
                Assert.IsTrue(workEmail.Length < 256);
                Assert.IsTrue(workEmail.Length > 5);
                Assert.IsTrue(workEmail.Length < 256);
                Assert.IsTrue(workEmail.EndsWith("@test.com"));
                StringAssert.Matches(workEmail, expectedEmailRegex);
            }
        }

        [TestMethod]
        public async Task CreateEmployee_GeneratesWorkPhone_WithCorrectFormat()
        {
            SetupCreateEmployeeMocks();

            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                var result = await _employeeService.CreateEmployee(_validDtoEmployee);
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.WorkPhone);
                string workPhone = result.WorkPhone;
                Assert.IsTrue(workPhone.Length > 7); 
                Assert.IsTrue(workPhone.Length < 26);
            }
        }

        [TestMethod()]
        public async Task GetAll_Success()
        {
            var result = await _employeeService.GetAll(null, null, null, null, 1, 10);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.TotalCount);
        }

        public async Task CreateEmployeeCompanyNotFound()
        {
            _mockCompanyRepository.Setup(x => x.GetById(1)).ReturnsAsync((Company?)null);
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
                async () => await _employeeService.CreateEmployee(_validDtoEmployee)
            );
        }

        [TestMethod()]
        [DataRow(true, true)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        public async Task GenerateUsernameWorkMailAndPhone_LoopTest(bool usernameExist, bool phoneExist)
        {
            _mockUserRepository.Setup(x => x.UsernameExists(It.IsAny<string>())).ReturnsAsync(usernameExist);
            _mockEmployeeRepository.Setup(x => x.PhoneNumExists(It.IsAny<string>())).ReturnsAsync(phoneExist);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _employeeService.CreateEmployee(_validDtoEmployee)
            );
        }


        [TestMethod()]
        [DataRow(null, null)]
        [DataRow("Bob", null)]
        [DataRow(null, "Doe")]
        [DataRow(" ", " ")]
        [DataRow(" ", null)]
        [DataRow(null, " ")]
        [DataRow("Bob", " ")]
        [DataRow(" ", "Doe")]
        public async Task CreateEmployee_WhitespaceOrNull(string fName, string Lname)
        {
            _validDtoEmployee.FirstName = fName;
            _validDtoEmployee.LastName = Lname;

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                async () => await _employeeService.CreateEmployee( _validDtoEmployee)
            );

        }

        [TestMethod()]
        public async Task CreateEmployee_InvalidFk()
        {
            _mockMapper.Setup(x => x.Map<AddEmployeeSpParams?>(It.IsAny<CreateNewEmployee>())).Returns(_validSpParamsEmployee);
            _mockEmployeeRepository
                .Setup(x => x.AddEmployeeSPAsync(It.IsAny<AddEmployeeSpParams>()))
                .ThrowsAsync(new KeyNotFoundException());

            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
                async () => await _employeeService.CreateEmployee(_validDtoEmployee)
            );
        }

        [TestMethod()]
        public async Task CreateEmployee_MapperReturnsNull_ThrowsInvalidOperationException()
        {
            _mockMapper.Setup(x => x.Map<AddEmployeeSpParams?>(It.IsAny<CreateNewEmployee>())).Returns((AddEmployeeSpParams?)null);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _employeeService.CreateEmployee(_validDtoEmployee)
            );
        }

        [TestMethod()]
        public async Task CreateEmployee_AddEmployeeSPAsyncReturnsNull_ThrowsInvalidOperationException()
        {
            _mockEmployeeRepository.Setup(x => x.AddEmployeeSPAsync(It.IsAny<AddEmployeeSpParams>())).ReturnsAsync((Employee?)null);
            _mockMapper.Setup(x => x.Map<AddEmployeeSpParams?>(It.IsAny<CreateNewEmployee>())).Returns(_validSpParamsEmployee);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _employeeService.CreateEmployee(_validDtoEmployee)
            );
        }

        [TestMethod()]
        public async Task CreateEmployee_EmployeeDtoMapperReturnsNull_ThrowsInvalidOperationException()
        {
            var createdEmployee = new Employee
            {
                Id = 10,
                FirstName = VALID_FIRST_NAME,
                LastName = VALID_LAST_NAME,
                WorkEmail = VALID_WORK_EMAIL,
                WorkPhone = VALID_WORK_PHONE,
                ImagePath = DEFAULT_IMAGE_PATH,
                DepartmentId = VALID_DEPARTMENT_ID,
                DiscProfileId = VALID_DISC_PROFILE_ID,
                PositionId = VALID_POSITION_ID
            };

            _mockEmployeeRepository.Setup(x => x.AddEmployeeSPAsync(It.IsAny<AddEmployeeSpParams>())).ReturnsAsync(createdEmployee);
            _mockMapper.Setup(x => x.Map<AddEmployeeSpParams?>(It.IsAny<CreateNewEmployee>())).Returns(_validSpParamsEmployee);
            _mockMapper.Setup(x => x.Map<EmployeeDto?>(It.IsAny<Employee>())).Returns((EmployeeDto?)null);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _employeeService.CreateEmployee(_validDtoEmployee)
            );
        }
    }
}