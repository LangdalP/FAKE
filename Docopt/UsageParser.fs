﻿namespace Docopt
#nowarn "62"
#light "off"

open FParsec
open System
open System.Text

type 'a GList = System.Collections.Generic.LinkedList<'a>

module USPH =
  begin
    let inline λ f a b = f (a, b)
    let inline Λ f (a, b) = f a b

    [<NoComparison>]
    type Ast =
      | Arg of string      // Argument
      | Sop of string      // Short option pack
      | Lop of string      // Long option
      | Cmd of string      // Command
      | Xor of (Ast * Ast) // Mutual exclusion (| operator)
      | Ell of Ast         // One or more (... operator)
      | Req of Ast         // Required (between parenthesis)
      | Opt of Ast         // Optional (between square brackets)
      | Ano                // Any options `[options]`
      | Dsh                // Double dash `[--]` (Bowser team is the best)
      | Ssh                // Single dash `[-]`
      | Seq of Ast GList   // Sequence
      | Eps                // Epsilon, always succeds and matches nothing

    let opp = OperatorPrecedenceParser<_, unit, unit>()
    let pupperArg =
      let start c' = isUpper c' || isDigit c' in
      let cont c' = start c' || c' = '-' in
      identifier (IdentifierOptions(isAsciiIdStart=start,
                                    isAsciiIdContinue=cont,
                                    label="UPPER-CASE identifier"))
    let plowerArg =
      satisfyL (( = ) '<') "<lower-case> identifier"
      >>. many1SatisfyL (( <> ) '>') "any character except '>'"
      .>> pchar '>'
      |>> (fun name' -> String.Concat("<", name', ">"))
    let parg = pupperArg <|> plowerArg
    let plop = 
      let predicate c' = isLetter(c') || isDigit(c') || c' = '-' || c' = '_'
      in skipString "--" >>. many1Satisfy predicate
    let psop =
      skipChar '-'
      >>. many1Satisfy (isLetter)
    let pcmd = many1Satisfy (fun c' -> isLetter(c') || isDigit(c'))
    let preq = between (pchar '(' >>. spaces) (pchar ')') opp.ExpressionParser
    let popt = between (pchar '[' >>. spaces) (pchar ']') opp.ExpressionParser
    let pano = skipString "[options]"
    let pdsh = skipString "[--]"
    let pssh = skipString "[-]"
    let term = choice [|
                        pano >>% Ano;
                        pdsh >>% Dsh;
                        pssh >>% Ssh;
                        popt |>> Opt;
                        preq |>> Req;
                        plop |>> Lop;
                        psop |>> Sop;
                        parg |>> Arg;
                        pcmd |>> Cmd;
                        |]
    let pxor = InfixOperator("|", spaces, 10, Associativity.Left, λ Xor)
    let pell = PostfixOperator("...", spaces, 20, false, Ell)
    let _ = let termParser =
              sepEndBy1 term spaces1
              |>> List.reduce (fun l' (r':Ast) ->
                                 match l' with
                                   | Seq(seq) -> let _ = seq.AddLast(r') in l'
                                   | _        -> Seq(GList<Ast>([l';r'])))
            in opp.TermParser <- termParser
    let _ = opp.AddOperator(pxor)
    let _ = opp.AddOperator(pell)
    let pusageLine = spaces >>. opp.ExpressionParser
  end
;;

open USPH
module Err = FParsec.Error

exception UsageException of string
exception ArgvException of string

type UsageParser(u':string, opts':Options) =
  class
    let parseAsync (line':string) = async {
        let line' = line'.TrimStart() in
        let line' = line'.Substring(line'.IndexOfAny([|' ';'\t'|])) in
        let _ = printfn "LINE = %s" line' in
        return match run pusageLine line' with
          | Success(res, _, _) -> res
          | Failure(err, _, _) -> raise (UsageException(err))
      }
    let ast = u'.Split([|'\n';'\r'|], StringSplitOptions.RemoveEmptyEntries)
              |> Seq.map parseAsync
              |> Async.Parallel
              |> Async.RunSynchronously
              |> Seq.reduce (λ Xor)
    let i = ref 0
    let len = ref 0
    let argv = ref<_ array> null
    let rec eval e = printfn "Eval: %A" e; match e with//function
      | Arg(arg) -> farg arg
      | Sop(sop) -> fsop sop
      | Lop(lop) -> flop lop
      | Cmd(cmd) -> fcmd cmd
      | Xor(xor) -> fxor xor
      | Ell(ast) -> fell ast
      | Req(ast) -> freq ast
      | Opt(ast) -> fopt ast
      | Ano      -> fano ( )
      | Dsh      -> fdsh ( )
      | Ssh      -> fssh ( )
      | Seq(seq) -> fseq seq
      | Eps      -> feps ( )
    and farg arg' = None
    and fsop sop' = None
    and flop lop' = None
    and fcmd cmd' = None
    and fxor xor' = None
    and fell ast' = None
    and freq ast' = None
    and fopt ast' = None
    and fano (  ) = None
    and fdsh (  ) = None
    and fssh (  ) = None
    and fseq seq' = None
    and feps (  ) = None
//      let e = ref None in
//      let pred ast' = match eval ast' with
//        | None -> false
//        | err  -> e := err; true
//      in if Seq.exists pred seq'
//      then !e
//      else None

    member __.Parse(argv':string array, args':Args) =
      i := 0;
      len := argv'.Length;
      argv := argv';
      printfn "Parsing: %A" ast;
      match eval ast with
        | _ when !i <> !len -> ArgvException("") |> raise
        | None              -> args'
        | Some(err)         -> let pos = FParsec.Position("", 0L, 0L, 0L) in
                               Err.ParserError(pos, null, err).ToString()
                               |> ArgvException
                               |> raise
    member __.Ast = ast
  end
;;
