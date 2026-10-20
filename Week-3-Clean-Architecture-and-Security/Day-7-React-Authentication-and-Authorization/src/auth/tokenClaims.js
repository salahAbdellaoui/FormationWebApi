export function readTokenUiClaims(token, nowSeconds = Date.now() / 1000) {
  if (typeof token !== "string") {
    return null;
  }

  const parts = token.split(".");
  if (parts.length !== 3 || parts.some((part) => part.length === 0)) {
    return null;
  }

  try {
    const base64Payload = parts[1]
      .replace(/-/g, "+")
      .replace(/_/g, "/")
      .padEnd(Math.ceil(parts[1].length / 4) * 4, "=");
    const binaryPayload = atob(base64Payload);
    const payloadBytes = Uint8Array.from(binaryPayload, (character) =>
      character.charCodeAt(0),
    );
    const payload = JSON.parse(new TextDecoder().decode(payloadBytes));

    if (
      payload === null ||
      typeof payload !== "object" ||
      Array.isArray(payload) ||
      typeof payload.exp !== "number" ||
      !Number.isFinite(payload.exp)
    ) {
      return null;
    }

    // The backend emits lowercase `role`, once per role; JSON can hold one string or an array.
    const roleClaim = payload.role;
    const roles =
      typeof roleClaim === "string"
        ? [roleClaim]
        : Array.isArray(roleClaim)
          ? roleClaim.filter((role) => typeof role === "string" && role.length > 0)
          : [];

    return {
      roles,
      isExpired: payload.exp <= nowSeconds,
    };
  } catch {
    return null;
  }
}
