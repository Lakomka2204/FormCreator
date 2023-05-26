using Microsoft.AspNetCore.Http.Extensions;

namespace Tests
{
    public class AuthTest
    {
        HttpClient _httpClient;
        [SetUp]
        public void Setup()
        {
            var ub = new UriBuilder()
            {
                Scheme = "https",
                Host = "localhost",
                Port = 7177,
            };
            _httpClient = new HttpClient()
            {
                BaseAddress = ub.Uri,
            };
        }

        [Test]
        public async Task Availability()
        {
            var r = await _httpClient.GetAsync("");
            Assert.That(r.IsSuccessStatusCode, Is.True);
        }
        [Test]
        public async Task TokenOK()
        {
            QueryBuilder qb = new QueryBuilder()
            {
                {"token", "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJJZCI6IjUwOGIwMzVkLTRjOTgtNDkzMS1hYzlhLTQwMmJmYWRmODc4NSIsImVtYWlsIjoicXdlcXdlQHoiLCJzdWIiOiJtYWEiLCJ1bmlxdWVfbmFtZSI6IkFRQUFBQUlBQVlhZ0FBQUFFRVIwcGJsZW1xQ2VnN3diS3BneS9mZmdoSXJoa3F0Zk94Z3ZXejRjOWFHWXowNHdmc25kVTZIU3U4OVRSWFpDWnc9PSIsImdlbmRlciI6IlRydWUiLCJqdGkiOiI3NGIxZDYzNi03MThlLTQ5NWItOWZhNS1iMzdhODFkYmNhNzIiLCJuYmYiOjE2ODQwODE4MzMsImV4cCI6MTY4Njc2MDIzMywiaWF0IjoxNjg0MDgxODMzLCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MTc3LyIsImF1ZCI6Imh0dHBzOi8vbG9jYWxob3N0OjcxNzcvIn0.6CsdgbpKkwIPBLIVZYrFb9Ce3ZyI4RHXU-SGoZ9FqpVXPhzcc1aGwThao4GNTR5lmYHqeEb9rDHpdFZ4C9UIgg"}
            };
            var r = await _httpClient.GetAsync($"api/auth/verifytoken{qb.ToQueryString()}");
            string sr = await r.Content.ReadAsStringAsync();
            Assert.Multiple(() =>
            {
                Assert.That(r.IsSuccessStatusCode, Is.True);
                Assert.That(sr, Does.Contain("Pass match"));
            });
        }
        [Test]
        public async Task TokenMissing()
        {
            var r = await _httpClient.GetAsync($"api/auth/verifytoken");
            string sr = await r.Content.ReadAsStringAsync();
            Assert.Multiple(() =>
            {
                Assert.That(r.IsSuccessStatusCode, Is.False);
                Assert.That(sr, Does.Contain("No token"));
            });
        }
        [Test]
        public async Task TokenInvalid()
        {
            QueryBuilder qb = new QueryBuilder()
            {
                {"token", "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJJZCI6IjUwOGIwMzVkLTRjOTgtNDkzMS1hYzlhLTQwMmJmYWRmODc4NSIsImVtYWlsIjoicXdlcXdlQHoiLCJzdWIiOiJtYWEiLCJ1bmlxdWVfbmFtZSI6IkFRQUFBQUlBQVlhZ0FBQUFFRVIwcGJsZW1xQ2VnN3diS3BneS9mZmdoSXJoa3F0Zk94Z3ZXejRjOWFHWXowNHdmc25kVTZIU3U4OVRSWFpDWnc9PSIsImdlbmRlciI6IlRydWUiLCJqdGkiOiI3NGIxZDYzNi03MThlLTQ5NWItOWZhNS1iMzdhODFkYmNhNzIiLCJuYmYiOjE2ODQwODE4MzMsImV4cCI6MTY4Njc2MDIzMywiaWF0IjoxNjg0MDgxODMzLCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo3MTc3LyIsImF1ZCI6Imh0dHBzOi8vbG9jYWxob3N0OjcxNzcvIn0.6CsdgbpKkwIPBLIVZYrFb9Ce3ZyI4RHXU-SGoZ9FqpVXPhzcc1aGwThao4GNTR5lmYHqeEb9rDHpdFZ4C9UIgz"}
            };
            var r = await _httpClient.GetAsync($"api/auth/verifytoken{qb.ToQueryString()}");
            string sr = await r.Content.ReadAsStringAsync();
            Assert.Multiple(() =>
            {
                Assert.That(r.IsSuccessStatusCode, Is.False);
                Assert.That(sr, Does.Contain("Invalid token"));
            });
        }
        [Test]
        public async Task PassMatch()
        {
            QueryBuilder qb = new QueryBuilder()
            {
                {"e","AQAAAAIAAYagAAAAEER0pblemqCeg7wbKpgy/ffghIrhkqtfOxgvWz4c9aGYz04wfsndU6HSu89TRXZCZw==" },
                {"d","asdasd" }
            };
            var r = await _httpClient.PostAsync($"api/auth/pwdcompare{qb.ToQueryString()}",null);
            Assert.Multiple(() =>
            {
                Assert.That(r.IsSuccessStatusCode, Is.True);
                Assert.That(r.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            });
        }
        [Test]
        public async Task PassMismatch()
        {
            QueryBuilder qb = new()
            {
                {"e","AQAAAAIAAYagAAAAEER0pblemqCeg7wbKpgy/ffghIrhkqtfOxgvWz4c9aGYz04wfsndU6HSu89TRXZCZw==" },
                {"d","asdasd1" }
            };
            var r = await _httpClient.PostAsync($"api/auth/pwdcompare{qb.ToQueryString()}",null);
            Assert.Multiple(() =>
            {
                Assert.That(r.IsSuccessStatusCode, Is.False);
                Assert.That(r.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
            });
        }
    }
}