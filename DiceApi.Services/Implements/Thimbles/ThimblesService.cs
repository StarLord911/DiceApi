using DiceApi.Common;
using DiceApi.Data.ApiReqRes.Thimble;
using DiceApi.Data.Data.Games;
using DiceApi.DataAcces.Repositoryes;
using DiceApi.DataAcces.Repositoryes.Game;
using DiceApi.Services.Contracts;
using EncryptingAndDecrypting;
using EncryptingAndDecrypting.Enums;
using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Implements.Thimbles
{
    public interface IThimblesService
    {
        Task<BetThimblesResponce> Bet(BetThimblesRequest request);

        void GetOrCreateHash(long userId);
    }

    public class ThimblesService : IThimblesService
    {
        private readonly IGamesRepository _gamesRepository;
        private readonly IUserService _userService;
        private readonly IWageringService _wageringService;
        private readonly ICacheService _cacheService;

        public ThimblesService(IGamesRepository gamesRepository,
            IUserService userService,
            ICacheService cacheService,
            IWageringService wageringService)
        {
            _gamesRepository = gamesRepository;
            _userService = userService;
            _cacheService = cacheService;
            _wageringService = wageringService;

        }

        public async Task<BetThimblesResponce> Bet(BetThimblesRequest request)
        {
            GetOrCreateHash(44);
            var user = _userService.GetById(request.UserId);

            if (user.Ballance < request.BetSum)
            {
                return new BetThimblesResponce()
                {
                    Message = "Нехватает денег.",
                    Succes = false,
                    Win = false
                };
            }

            if (request.BetSum > 1000)
            {
                return new BetThimblesResponce()
                {
                    Message = "Максимальная ставка 1000.",
                    Succes = false,
                    Win = false
                };
            }

            await _wageringService.UpdatePlayed(user.Id, request.BetSum);

            await _userService.UpdateUserBallance(request.UserId, user.Ballance - request.BetSum);

            var game = new GameModel()
            {
                UserId = user.Id,
                BetSum = request.BetSum,
                CanWin = request.BetSum * 3,
                GameType = Data.GameType.Thimbles,
                GameTime = DateTime.UtcNow.GetMSKDateTime()
            };

            var randoom = new MersenneTwister().Next(1, 3);

            if (randoom == 1)
            {
                await _userService.UpdateUserBallance(request.UserId, user.Ballance + (request.BetSum * 3));

                game.Win = true;
                await _gamesRepository.AddGame(game);

                return new BetThimblesResponce()
                {
                    Message = $"Вы выиграли {Math.Round(request.BetSum * 3, 2)}",
                    Succes = true,
                    Win = true
                };
            }

            game.Win = false;
            await _gamesRepository.AddGame(game);

            return new BetThimblesResponce()
            {
                Message = $"Вы проиграли",
                Succes = true,
                Win = false
            };

        }

        public void GetOrCreateHash(long userId)
        {
            var rsaKeys = GenerateRsaKeys();
            string privateKeyPem = rsaKeys.PrivateKey;
            string publicKeyPem = rsaKeys.PublicKey;

            string original = "Секретное сообщение";

            var encrypted = EncryptString(original, publicKeyPem);
            string decrypted = DecryptString(encrypted, privateKeyPem);

            
        }

        public static (string PrivateKey, string PublicKey) GenerateRsaKeys()
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = 2048;

                string publicKeyPem = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                string privateKeyPem = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

                return (privateKeyPem, publicKeyPem);
            }
        }

        public static string EncryptString(string inputString, string publicKeyPem)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKeyPem), out _);
                byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);
                byte[] encryptedBytes = rsa.Encrypt(inputBytes, RSAEncryptionPadding.OaepSHA1);
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        public static string DecryptString(string inputBytes, string privateKeyPem)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKeyPem), out _);
                byte[] decryptedBytes = rsa.Decrypt(Convert.FromBase64String(inputBytes), RSAEncryptionPadding.OaepSHA1);
                string decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedData;
            }
        }
    }
        
}
