let keyv;
const setTokenService = service => (keyv = service);
const setRefreshToken = async (id, refresh_token) => keyv.set(String(id), refresh_token);
const getRefreshToken = async id => keyv.get(String(id));
const revokeRefreshToken = async id => keyv.delete(String(id));

const setUserService = () => {};
const setRefreshTokenStoreName = () => {};
const setAuthUserStoreName = () => {};
const findUser = () => {};
const updateUser = () => {};

export {
  findUser,
  getRefreshToken,
  revokeRefreshToken,
  setAuthUserStoreName,
  setRefreshToken,
  setRefreshTokenStoreName,
  setTokenService,
  setUserService,
  updateUser,
};
