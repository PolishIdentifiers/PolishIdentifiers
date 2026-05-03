# MCP Servers

This page describes the [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) servers that are useful when working on or with this library. MCP servers extend AI coding assistants (such as Claude, Cursor, or GitHub Copilot) with additional tools — giving them access to live documentation, code search, file reading, and more.

## context7

**Source:** [`@upstash/context7-mcp`](https://github.com/upstash/context7)

Fetches real-time, version-specific documentation and code examples for libraries and frameworks directly from their official sources (GitHub READMEs, npm tarballs). Injects current API docs into the AI's context so it uses accurate, up-to-date information instead of stale training data.

**Key tools:**

- `resolve-library-id` — maps a library name (e.g. `"react"`) to a Context7 queryable ID
- `query-docs` — retrieves documentation for the identified library, filtered by version and query

**Example config:**

```json
{
  "mcpServers": {
    "context7": {
      "command": "npx",
      "args": ["-y", "@upstash/context7-mcp@latest"]
    }
  }
}
```

## grep_app

**Source:** [`grep_app_mcp`](https://github.com/ai-tools-all/grep_app_mcp) / [Official hosted endpoint](https://mcp.grep.app)

Provides full-text and regex code search across millions of public GitHub repositories via the [grep.app](https://grep.app) API. Useful for finding real-world usage examples of any API, pattern, or identifier format.

**Key tool:**

- `searchCode` — searches public repos with optional filters for language, repository, and file path

**Example config (remote):**

```json
{
  "mcpServers": {
    "grep_app": {
      "url": "https://mcp.grep.app"
    }
  }
}
```

## pdf-mcp

**Source:** [`jztan/pdf-mcp`](https://github.com/jztan/pdf-mcp) / [`pip install pdf-mcp`](https://pypi.org/project/pdf-mcp/)

Allows AI agents to read, search, and extract content from PDF files. Reads documents in chunks to stay within LLM context limits. Supports hybrid search (BM25 + semantic), OCR for scanned documents, and structured extraction of tables and images.

**Key tools:**

- `pdf_info` — returns metadata: page count, file size, estimated tokens
- `pdf_read_pages` — reads specific pages with optional OCR
- `pdf_search` — searches a PDF using natural language

**Example config:**

```json
{
  "mcpServers": {
    "pdf-mcp": {
      "command": "pdf-mcp-server"
    }
  }
}
```

## sequential-thinking

**Source:** [`@modelcontextprotocol/server-sequential-thinking`](https://www.npmjs.com/package/@modelcontextprotocol/server-sequential-thinking)

Enables structured, step-by-step reasoning. The AI breaks a complex problem into an explicit sequence of numbered thoughts, can revise earlier steps, and produces a final answer only after working through the full chain. Particularly effective for debugging, algorithm design, and multi-step analysis.

**Example config:**

```json
{
  "mcpServers": {
    "sequential-thinking": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-sequential-thinking"]
    }
  }
}
```

## serena

**Source:** [`oraios/serena`](https://github.com/oraios/serena)

An open-source MCP toolkit that gives an AI coding agent IDE-level capabilities by connecting to the Language Server Protocol (LSP). Provides semantic code search, symbol navigation, cross-file refactoring, and precise edits — going beyond plain-text retrieval. Supports 20+ languages and integrates with VS Code, JetBrains, Cursor, and Claude Desktop/Code.

**Example config:**

```json
{
  "mcpServers": {
    "serena": {
      "command": "uvx",
      "args": [
        "--from", "git+https://github.com/oraios/serena",
        "serena", "start-mcp-server",
        "--context", "ide-assistant",
        "--project", "/path/to/project"
      ]
    }
  }
}
```

## stitch

**Source:** [`davideast/stitch-mcp`](https://github.com/davideast/stitch-mcp) and related forks

Bridges AI coding agents with [Google Stitch](https://stitch.withgoogle.com/) — a generative UI design tool. The MCP server exposes design context, screen code, and images from Stitch projects so that agents can turn AI-generated UI designs directly into production code for frameworks such as React, Svelte, or Vue.

**Key tools (vary by implementation):**

- extract design context from a project
- fetch screen code and images
- generate new screens from a text prompt

**Example config:**

```json
{
  "mcpServers": {
    "stitch": {
      "command": "npx",
      "args": ["@_davideast/stitch-mcp", "serve", "-p", "<project-id>"]
    }
  }
}
```

## websearch

**Source:** [`@modelcontextprotocol/server-brave-search`](https://www.npmjs.com/package/@modelcontextprotocol/server-brave-search) / [`tavily-ai/tavily-mcp`](https://github.com/tavily-ai/tavily-mcp)

Gives the AI model real-time web search capabilities beyond its training cut-off. Two common implementations:

- **Brave Search MCP** — privacy-focused web and local business search via the [Brave Search API](https://search.brave.com/api)
- **Tavily MCP** — research-oriented search with dedicated tools for crawling, extraction, and site mapping

**Example config (Brave Search):**

```json
{
  "mcpServers": {
    "websearch": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-brave-search"],
      "env": {
        "BRAVE_API_KEY": "<your-api-key>"
      }
    }
  }
}
```
